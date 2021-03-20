using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Smith.MatrixSdk.ApiTypes;
using Smith.MatrixSdk.Extensions;

namespace Smith.MatrixSdk
{
    public class MatrixClient
    {
        internal static readonly JsonSerializerSettings JsonSettings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            DateParseHandling = DateParseHandling.None
        };

        [PublicAPI] protected readonly HttpClient HttpClient;
        [PublicAPI] protected readonly ILogger Logger;

        public MatrixClient(ILogger logger, HttpClient httpClient, Uri homeserverUri)
        {
            Logger = logger;
            HttpClient = httpClient;
            HttpClient.BaseAddress = homeserverUri;
        }

        public Task<LoginResponse> Login(string user, string password, CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Sending a login request for user {Login}", user);
            var request = new LoginRequest(MatrixApiConstants.LoginPasswordType, password, user);
            return Post<LoginRequest, LoginResponse>(MatrixApiUris.Login, request, cancellationToken);
        }

        /// <summary>Start event polling asynchronously.</summary>
        /// <returns>The observable event sequence. Will terminate on first error.</returns>
        public IObservable<SyncResponse> StartEventPolling(string accessToken, TimeSpan longPollingTimeout) =>
            Observable.Create<SyncResponse>(async (observer, cancellationToken) =>
            {
                var request = new SyncRequest(Timeout: (int)longPollingTimeout.TotalMilliseconds);
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var response = await SyncEvents(accessToken, request, cancellationToken);
                        Logger.LogTrace("Sync result: {Response}", response);
                        request = request with {Since = response.NextBatch};
                        observer.OnNext(response);
                    }
                    catch (TaskCanceledException)
                    {
                        Logger.LogInformation("Event polling cancelled");
                        observer.OnCompleted();
                        return;
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                        return;
                    }
                }
            });

        private Task<SyncResponse> SyncEvents(
            string accessToken,
            SyncRequest request,
            CancellationToken cancellationToken)
        {
            return Get<SyncResponse>(MatrixApiUris.Sync, accessToken, new Dictionary<string, string?>
            {
                ["filter"] = request.Filter,
                ["since"] = request.Since,
                ["full_state"] = request.FullState?.ToString(CultureInfo.InvariantCulture).ToLowerInvariant(),
                ["set_presence"] = request.SetPresence?.ToString().ToLowerInvariant(),
                ["timeout"] = request.Timeout?.ToString(CultureInfo.InvariantCulture)
            }.FilterNotNull(), cancellationToken, true);
        }

        [PublicAPI]
        protected async Task<TResponse> Get<TResponse>(
            string baseUri,
            string accessToken,
            Dictionary<string, string> queryParameters,
            CancellationToken cancellationToken,
            bool debugLogContents) where TResponse : class
        {
            var uri = QueryHelpers.AddQueryString(baseUri, queryParameters);
            var request = new HttpRequestMessage(HttpMethod.Get, uri)
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", accessToken)
                }
            };

            var response = await HttpClient.SendAsync(request, cancellationToken);
            Logger.LogTrace("Result of get request {Uri}: {Result}", uri, response.StatusCode);

            var content = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync(cancellationToken);
            Logger.LogDebug("Contents of {Uri}: {Contents}", uri, content);
            return JsonConvert.DeserializeObject<TResponse>(content, JsonSettings).NotNull();
        }

        [PublicAPI]
        protected async Task<TResponse> Post<TRequest, TResponse>(
            string uri,
            TRequest requestData,
            CancellationToken cancellationToken) where TResponse : class
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(requestData, Formatting.None, JsonSettings),
                    Encoding.UTF8,
                    "application/json")
            };
            var response = await HttpClient.SendAsync(request, cancellationToken);
            Logger.LogTrace("Result of post request {Uri}: {Result}", uri, response.StatusCode);
            return JsonConvert.DeserializeObject<TResponse>(
                await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync(cancellationToken),
                JsonSettings).NotNull();
        }
    }
}
