using System.Threading.Tasks;
using ConsoleAppFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Smith.MatrixSdk.Example
{
    public static class Program
    {
        internal static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .RunConsoleAppFrameworkAsync<Application>(args);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
        }
    }
}
