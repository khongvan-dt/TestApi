using Microsoft.AspNetCore.Mvc;
using TLS.TSC.AutoApiTester.Services;
using TLS.TSC.AutoApiTester.Models;

namespace TLS.TSC.AutoApiTester.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestsController : ControllerBase
{
    private readonly IPlaywrightTestRunner _runner;
    private readonly IWebHostEnvironment _env;

    public TestsController(IPlaywrightTestRunner runner, IWebHostEnvironment env)
    {
        _runner = runner;
        _env = env;
    }

    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "ok" });

    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] RunRequest req)
    {
        // req.FilePath is relative to backend/TestData
        var baseDir = Path.Combine(AppContext.BaseDirectory, "TestData");
        var fullPath = Path.Combine(baseDir, req.FilePath ?? "");
        if (!System.IO.File.Exists(fullPath))
            return NotFound(new { error = "File not found", path = fullPath });

        var logs = Path.Combine(AppContext.BaseDirectory, "Logs");
        var runner = (PlaywrightTestRunner)_runner;
        await runner.RunAsync(fullPath, Path.GetFileNameWithoutExtension(fullPath), req.BaseUrl ?? "http://localhost:5000", req.SharedToken);
        // read latest log file
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        var reportFile = Path.Combine(logs, today, $"{Path.GetFileNameWithoutExtension(fullPath)}.json");
        if (System.IO.File.Exists(reportFile))
        {
            var content = await System.IO.File.ReadAllTextAsync(reportFile);
            return Content(content, "application/json");
        }
        return Ok(new { message = "Run finished, but no report found." });
    }
}

public class RunRequest
{
    public string? FilePath { get; set; }
    public string? BaseUrl { get; set; }
    public string? SharedToken { get; set; }
}
