using AutoApiTester.Models;

public class Request
{
    public int Id { get; set; }
    public int CollectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? AuthType { get; set; }
    public string? AuthValue { get; set; }

    // Test data gộp vào đây
    public string? TestDataName { get; set; }
    public string? TestDataContent { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation
    public Collection? Collection { get; set; }
    public ICollection<RequestHeader> RequestHeaders { get; set; } = new List<RequestHeader>();
    public ICollection<RequestParam> RequestParams { get; set; } = new List<RequestParam>();
    public ICollection<RequestBody> RequestBodies { get; set; } = new List<RequestBody>();
}
