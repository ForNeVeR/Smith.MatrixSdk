using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Smith.MatrixSdk.ApiTypes;

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
            Logger.LogTrace("Result of response {Uri}: {Result}", uri, response.StatusCode);
            return JsonConvert.DeserializeObject<TResponse>(
                await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync(cancellationToken),
                JsonSettings).NotNull();
        }
    }
}
