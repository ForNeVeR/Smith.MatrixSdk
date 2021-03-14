using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
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

        public MatrixClient(ILogger logger, HttpClient httpClient, string homeserver)
        {
            Logger = logger;
            HttpClient = httpClient;
            HttpClient.BaseAddress = new Uri(homeserver);
        }

        public Task<LoginResponse> Login(string user, string password, CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Sending a login request for user {Login}", user);
            var request = new LoginRequest(password, MatrixApiConstants.LoginPasswordType, user);
            return Post<LoginRequest, LoginResponse>(MatrixApiUris.Login, request, cancellationToken);
        }

        /// <summary>Start event polling asynchronously.</summary>
        /// <param name="authToken">Authentication token to send to the server.</param>
        /// <returns>The observable event sequence. Will terminate on first error.</returns>
        public IObservable<SyncResponse> StartEventPolling(string authToken) =>
            Observable.Create<SyncResponse>(async (observer, cancellationToken) =>
            {
                var request = new SyncRequest();
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var response = await SyncEvents(authToken, request, cancellationToken);
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
            string authToken,
            SyncRequest request,
            CancellationToken cancellationToken)
        {
            return Get<SyncResponse>(MatrixApiUris.Sync, authToken, new Dictionary<string, string?>
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
            string authToken,
            Dictionary<string, string> queryParameters,
            CancellationToken cancellationToken,
            bool debugLogContents) where TResponse : class
        {
            var uri = QueryHelpers.AddQueryString(baseUri, queryParameters);
            var request = new HttpRequestMessage(HttpMethod.Get, uri)
            {
                Headers =
                {
                    {"Authorization", $"Bearer: {authToken}"}
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
