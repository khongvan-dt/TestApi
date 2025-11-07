using AutoApiTester.Models;

public class RequestEntity
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
    public CollectionEntity? Collection { get; set; }
    public ICollection<RequestHeaderEntity> RequestHeaders { get; set; } = new List<RequestHeaderEntity>();
    public ICollection<RequestParamEntity> RequestParams { get; set; } = new List<RequestParamEntity>();
    public ICollection<RequestBodyEntity> RequestBodies { get; set; } = new List<RequestBodyEntity>();
}
