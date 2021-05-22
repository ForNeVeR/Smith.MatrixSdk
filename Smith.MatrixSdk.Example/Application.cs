using System;
using System.Net.Http;
using System.Threading.Tasks;
using ConsoleAppFramework;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Smith.MatrixSdk.Extensions;

namespace Smith.MatrixSdk.Example
{
    internal class Application : ConsoleAppBase
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public Application(ILogger<Application> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        [UsedImplicitly]
        public async Task Run(string user, string password, string homeserver = "https://matrix.org")
        {
            var client = new MatrixClient(_logger, _httpClient, new Uri(homeserver));
            var loginResponse = await client.Login(user, password);
            Console.WriteLine(loginResponse);

            var events = client.StartEventPolling(loginResponse.AccessToken.NotNull(), TimeSpan.FromSeconds(5));
            events.Subscribe(
                syncResponse => Console.WriteLine($"Next batch: {syncResponse.NextBatch}"),
                Context.CancellationToken
            );
            await Context.CancellationToken.WhenCancelled();
        }
    }
}
