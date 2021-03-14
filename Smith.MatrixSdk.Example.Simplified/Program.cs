using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;

namespace Smith.MatrixSdk.Example.Simplified
{
    public static class Program
    {
        private static string AskString(string title)
        {
            Console.Write($"{title}: ");
            return Console.ReadLine()!;
        }

        public static async Task Main()
        {
            var logger = NullLogger.Instance;
            using var httpClient = new HttpClient();
            var client = new MatrixClient(logger, httpClient, "https://matrix.org");

            var user = AskString("User");
            var password = AskString("Password");
            var loginResponse = await client.Login(user, password);

            Console.WriteLine(loginResponse.AccessToken);
        }
    }
}
