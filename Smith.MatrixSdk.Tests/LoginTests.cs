using System.IO;
using System.Text;
using System.Threading.Tasks;
using Extensions.Logging.NUnit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using Smith.MatrixSdk.ApiTypes;

namespace Smith.MatrixSdk.Tests
{
    public class LoginTests
    {
        private static readonly ILogger Logger = new NUnitLogger(nameof(LoginTests));

        [Test]
        public async Task SerializationTest()
        {
            const string homeserverHost = "example.com";
            const string user = "cheeky_monkey";
            const string password = "the_password";

            var httpHandler = new MockHttpMessageHandler();
            httpHandler
                .Expect(MatrixApiUris.Login)
                .Respond("application/json", request =>
                {
                    var content = request.Content!.ReadAsStringAsync().Result;
                    var requestData = JsonConvert.DeserializeObject<LoginRequest>(content, MatrixClient.JsonSettings);

                    Assert.AreEqual(
                        new LoginRequest(password, MatrixApiConstants.LoginPasswordType, user),
                        requestData);

                    return new MemoryStream(Encoding.UTF8.GetBytes($@"{{
  ""user_id"": ""@{user}:{homeserverHost}"",
  ""access_token"": ""abc123"",
  ""home_server"": ""{homeserverHost}""
}}"));
                });
            using var httpClient = httpHandler.ToHttpClient();
            var client = new MatrixClient(Logger, httpClient, "https://example.com");
            var result = await client.Login(user, password);

            Assert.AreEqual(
                new LoginResponse(
                    UserId: $"@{user}:{homeserverHost}",
                    AccessToken: "abc123",
                    HomeServer: homeserverHost),
                result);
            httpHandler.VerifyNoOutstandingExpectation();
        }
    }
}
