using Newtonsoft.Json;

namespace TLS.TSC.AutoApiTester.Models;

public class EndpointGroup
{
    [JsonProperty("endpoint")]
    public string Endpoint { get; set; } = "";

    [JsonProperty("method")]
    public string Method { get; set; } = "GET";

    [JsonProperty("headers")]
    public Dictionary<string, string>? Headers { get; set; }

    [JsonProperty("dataBase")]
    public object? DataBase { get; set; }

    [JsonProperty("testCases")]
    public List<object>? TestCases { get; set; }
}

public class ApiResponse
{
    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("message")]
    public string? Message { get; set; }

    [JsonProperty("data")]
    public object? Data { get; set; }
}

public class TestResult
{
    [Newtonsoft.Json.JsonIgnore]
    public bool Passed { get; set; }

    [JsonProperty("TestName")]
    public string TestName { get; set; } = "";

    [JsonProperty("ApiStatus")]
    public int? ApiStatus { get; set; }

    [JsonProperty("Message")]
    public string Message { get; set; } = "";

    [JsonProperty("RequestBody")]
    public string? RequestBody { get; set; }

    [JsonProperty("ResponseBody")]
    public string? ResponseBody { get; set; }
}

public class TestRun
{
    [JsonProperty("Time")]
    public DateTime Time { get; set; }

    [JsonProperty("Total")]
    public int Total { get; set; }

    [JsonProperty("Passed")]
    public int Passed { get; set; }

    [JsonProperty("Failed")]
    public int Failed { get; set; }

    [JsonProperty("Results")]
    public List<TestResult> Results { get; set; } = new();
}

public class TestReport
{
    [JsonProperty("Controller")]
    public string Controller { get; set; } = "";

    [JsonProperty("Date")]
    public string Date { get; set; } = "";

    [JsonProperty("TestRuns")]
    public List<TestRun> TestRuns { get; set; } = new();
}
