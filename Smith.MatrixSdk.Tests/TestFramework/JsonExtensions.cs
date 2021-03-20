using System.IO;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Smith.MatrixSdk.Extensions;

namespace Smith.MatrixSdk.Tests.TestFramework
{
    public static class JsonExtensions
    {
        public static Stream ToStream(this string str) => new MemoryStream(Encoding.UTF8.GetBytes(str));

        public static string SerializeToString<T>(this T value) =>
            JsonConvert.SerializeObject(value, Formatting.Indented, MatrixClient.JsonSettings);

        public static Stream SerializeToStream<T>(this T value) => value.SerializeToString().ToStream();

        public static T DeserializeContent<T>(this HttpRequestMessage request) where T : class
        {
            var content = request.Content.NotNull().ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(content, MatrixClient.JsonSettings).NotNull();
        }
    }
}
