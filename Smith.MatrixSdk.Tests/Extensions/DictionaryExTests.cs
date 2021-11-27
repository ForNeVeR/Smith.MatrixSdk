using System.Collections.Generic;
using NUnit.Framework;
using Smith.MatrixSdk.Extensions;

namespace Smith.MatrixSdk.Tests.Extensions
{
    public class DictionaryExTests
    {
        [Test]
        public void TestFilterNotNull()
        {
            var d = new Dictionary<string, string?>
            {
                ["a"] = "a",
                ["b"] = null
            };
            Assert.AreEqual(new Dictionary<string, string?> { ["a"] = "a" }, d.FilterNotNull());
        }
    }
}
