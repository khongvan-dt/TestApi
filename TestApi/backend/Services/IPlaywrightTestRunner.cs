using System.Threading.Tasks;

namespace TLS.TSC.AutoApiTester.Services;

public interface IPlaywrightTestRunner
{
    string? GetCurrentToken();
    Task RunAsync(string configPath, string controllerName, string baseUrl, string? sharedToken = null);
}
