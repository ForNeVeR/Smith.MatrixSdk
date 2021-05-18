using System;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
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
        private readonly CancellationTokenSource _ctrl_c_cts;

        public Application(ILogger<Application> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _ctrl_c_cts = new CancellationTokenSource();

            Console.CancelKeyPress += delegate {
                _ctrl_c_cts.Cancel();
            };
        }

        [UsedImplicitly]
        public async Task Run(string user, string password, string homeserver = "https://matrix.org")
        {
            var client = new MatrixClient(_logger, _httpClient, new Uri(homeserver));
            var loginResponse = await client.Login(user, password, _ctrl_c_cts.Token);
            Console.WriteLine(loginResponse);

            await client.StartEventPolling(loginResponse.AccessToken.NotNull(), TimeSpan.FromSeconds(5))
                .Do(syncResponse =>
                {
                    Console.WriteLine($"Next batch: {syncResponse.NextBatch}");

                    _ctrl_c_cts.Token.ThrowIfCancellationRequested();
                });
        }
    }
}
