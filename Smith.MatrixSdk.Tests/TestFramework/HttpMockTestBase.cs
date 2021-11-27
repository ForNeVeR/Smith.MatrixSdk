using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace Smith.MatrixSdk.Tests.TestFramework
{
    public class HttpMockTestBase
    {
        protected MockHttpMessageHandler HttpHandler = new();

        [TearDown]
        public void TearDown()
        {
            HttpHandler.VerifyNoOutstandingExpectation();
            HttpHandler.Dispose();
        }
    }
}
