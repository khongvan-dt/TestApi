using Microsoft.Playwright;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TLS.TSC.AutoApiTester.Models;

namespace TLS.TSC.AutoApiTester.Services;

public class PlaywrightTestRunner : IPlaywrightTestRunner
{
    private readonly string _logsFolder;
    private string? _currentAccessToken;  // Token dung chung giua cac file test

    public PlaywrightTestRunner()
    {
        _logsFolder = Path.Combine(AppContext.BaseDirectory, "Logs");
        Directory.CreateDirectory(_logsFolder);
    }

    public string? GetCurrentToken() => _currentAccessToken;

    public async Task RunAsync(string configPath, string controllerName, string baseUrl, string? sharedToken = null)
    {
        _currentAccessToken = sharedToken ?? _currentAccessToken;
        var config = JsonConvert.DeserializeObject<EndpointGroup>(await File.ReadAllTextAsync(configPath));
        if (config?.TestCases == null) return;

        using var playwright = await Playwright.CreateAsync();
        await using var request = await playwright.APIRequest.NewContextAsync(new() { BaseURL = baseUrl });

        var results = new List<TestResult>();
        for (int i = 0; i < config.TestCases.Count; i++)
        {
            var result = await CallApiAsync(request, config, config.TestCases[i], i + 1);
            results.Add(result);
            var status = result.Passed ? "PASS" : $"FAIL (API {result.ApiStatus})";
            Console.WriteLine($"  {status} - {result.TestName}");
        }

        SaveLog(controllerName, results);
    }

    private async Task<TestResult> CallApiAsync(IAPIRequestContext request, EndpointGroup config, object testCase, int index)
    {
        try
        {
            var testObj = JObject.FromObject(testCase);
            var testName = testObj["testName"]?.Value<string>() ?? testObj["name"]?.Value<string>();
            var saveToken = testObj["saveToken"]?.Value<bool>() ?? false;

            testObj.Remove("testName");
            testObj.Remove("saveToken");
            testObj.Remove("name");

            var hasData = testObj.HasValues;

            object? finalBody;
            if (!hasData)
            {
                finalBody = config.DataBase;
            }
            else if (config.DataBase != null)
            {
                var baseJson = JObject.FromObject(config.DataBase);
                baseJson.Merge(testObj, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Replace,
                    MergeNullValueHandling = MergeNullValueHandling.Merge
                });
                finalBody = baseJson;
            }
            else
            {
                finalBody = testObj;
            }

            var jsonBody = JsonConvert.SerializeObject(finalBody);
            var headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" };
            if (config.Headers != null)
            {
                foreach (var h in config.Headers)
                {
                    if (h.Key == "Authorization" && h.Value == "Bearer " && !string.IsNullOrEmpty(_currentAccessToken))
                        headers[h.Key] = $"Bearer {_currentAccessToken}";
                    else
                        headers[h.Key] = h.Value;
                }
            }

            var response = config.Method.ToUpper() switch
            {
                "POST" => await request.PostAsync(config.Endpoint, new() { Data = jsonBody, Headers = headers }),
                "GET" => await request.GetAsync(config.Endpoint, new() { Headers = headers }),
                "PUT" => await request.PutAsync(config.Endpoint, new() { Data = jsonBody, Headers = headers }),
                "DELETE" => await request.DeleteAsync(config.Endpoint, new() { Headers = headers }),
                _ => throw new NotSupportedException($"Method {config.Method} not supported")
            };

            var responseBody = await response.TextAsync();
            var apiResponse = string.IsNullOrWhiteSpace(responseBody) ? null : JsonConvert.DeserializeObject<ApiResponse>(responseBody);

            if (saveToken && response.Status == 200 && apiResponse?.Status == 200)
            {
                var json = JObject.Parse(responseBody);
                var token = json["data"]?["accessToken"]?.ToString()
                         ?? json["data"]?["access_token"]?.ToString()
                         ?? json["accessToken"]?.ToString()
                         ?? json["access_token"]?.ToString()
                         ?? json["token"]?.ToString();

                if (!string.IsNullOrEmpty(token))
                {
                    _currentAccessToken = token;
                    Console.WriteLine($"    Token saved: {token.Substring(0, Math.Min(20, token.Length))}...");
                }
            }

            var passed = response.Status == 200 && (apiResponse?.Status == 200 || apiResponse?.Status == null);

            return new TestResult
            {
                TestName = testName ?? $"Test {index}",
                Passed = passed,
                ApiStatus = apiResponse?.Status,
                Message = apiResponse?.Message ?? "",
                RequestBody = jsonBody,
                ResponseBody = responseBody
            };
        }
        catch (Exception ex)
        {
            return new TestResult
            {
                TestName = $"Test {index}",
                Passed = false,
                Message = ex.Message,
                RequestBody = null,
                ResponseBody = null
            };
        }
    }

    private void SaveLog(string controllerName, List<TestResult> results)
    {
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        var reportFile = Path.Combine(_logsFolder, today, $"{controllerName}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(reportFile)!);

        var report = File.Exists(reportFile)
            ? JsonConvert.DeserializeObject<TestReport>(File.ReadAllText(reportFile))
            : null;

        report ??= new TestReport { Controller = controllerName, Date = today, TestRuns = new() };
        report.TestRuns ??= new();

        report.TestRuns.Add(new TestRun
        {
            Time = DateTime.Now,
            Total = results.Count,
            Passed = results.Count(r => r.Passed),
            Failed = results.Count(r => !r.Passed),
            Results = results
        });

        File.WriteAllText(reportFile, JsonConvert.SerializeObject(report, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Include,
            DateFormatString = "yyyy-MM-dd HH:mm:ss"
        }));

        var passed = results.Count(r => r.Passed);
        Console.WriteLine($"\n{new string('=', 60)}");
        Console.WriteLine($"{controllerName} - Total: {results.Count} | Pass: {passed} | Fail: {results.Count - passed}");
        Console.WriteLine($"Log: {today}/{controllerName}.json");
        Console.WriteLine($"{new string('=', 60)}");
    }
}
