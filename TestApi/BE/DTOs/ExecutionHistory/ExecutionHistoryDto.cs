namespace AutoApiTester.Models.DTOs;

// ✅ Response DTO - Hiển thị trong list
public class ExecutionHistoryResponseDto
{
    public int Id { get; set; }
    public int? RequestId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public string? StatusText { get; set; }
    public int? ResponseTime { get; set; }
    public DateTime ExecutedAt { get; set; }
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public string? ErrorMessage { get; set; }
}

// ✅ Detail DTO - Xem chi tiết 1 execution
public class ExecutionHistoryDetailDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? RequestId { get; set; }

    // Request
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
    public int? ResponseTime { get; set; }

    // Error
    public string? ErrorMessage { get; set; }

    // Metadata
    public DateTime ExecutedAt { get; set; }
}

// ✅ Create DTO - Lưu lịch sử sau khi execute
public class CreateExecutionHistoryDto
{
    public int UserId { get; set; }
    public int? RequestId { get; set; }

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
    public int? ResponseTime { get; set; }

    // Error
    public string? ErrorMessage { get; set; }

    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}