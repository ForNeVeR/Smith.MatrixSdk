using System;
using System.Net.Http;
using System.Threading.Tasks;
using ConsoleAppFramework;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

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
            var client = new MatrixClient(_logger, _httpClient, homeserver);
            var response = await client.Login(user, password);
            Console.WriteLine(response);
        }
    }
}
