using System.Threading.Tasks;
using Extensions.Logging.NUnit;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using Smith.MatrixSdk.ApiTypes;
using Smith.MatrixSdk.Tests.TestFramework;
using static Smith.MatrixSdk.Tests.TestFramework.TestConstants;

namespace Smith.MatrixSdk.Tests
{
    public class LoginTests : HttpMockTestBase
    {
        private static readonly ILogger Logger = new NUnitLogger(nameof(LoginTests));

        [Test]
        public async Task DeserializationTest()
        {
            const string user = "cheeky_monkey";
            const string password = "the_password";

            HttpHandler
                .Expect(MatrixApiUris.Login)
                .Respond("application/json", request =>
                {
                    var loginRequest = request.DeserializeContent<LoginRequest>();

                    Assert.AreEqual(
                        new LoginRequest(Type: MatrixApiConstants.LoginPasswordType, Password: password, User: user),
                        loginRequest);

                    return new LoginResponse(
                        UserId: $"@{user}:{HomeserverHost}",
                        AccessToken: "abc123",
                        HomeServer: HomeserverHost).SerializeToStream();
                });
            using var httpClient = HttpHandler.ToHttpClient();
            var client = new MatrixClient(Logger, httpClient, HomeserverUri);
            var result = await client.Login(user, password);

            Assert.AreEqual(
                new LoginResponse(
                    UserId: $"@{user}:{HomeserverHost}",
                    AccessToken: "abc123",
                    HomeServer: HomeserverHost),
                result);
        }
    }
}
