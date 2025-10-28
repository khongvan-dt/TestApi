namespace AutoApiTester.Models;

public class ExecutionHistory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? RequestId { get; set; } // Nullable nếu request đã bị xóa

    // Request snapshot
    public string Method { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Headers { get; set; }
    public string? QueryParams { get; set; }
    public string? Body { get; set; }

    // Response
    public int? StatusCode { get; set; }
    public string? StatusText { get; set; }
    public string? ResponseHeaders { get; set; }
    public string? ResponseBody { get; set; }
    public int? ResponseTime { get; set; } // Milliseconds

    // Error
    public string? ErrorMessage { get; set; }

    // Metadata
    public DateTime ExecutedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public User? User { get; set; }
    public Request? Request { get; set; }
}