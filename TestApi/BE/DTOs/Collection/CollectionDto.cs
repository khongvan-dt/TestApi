namespace AutoApiTester.Models.DTOs;

// ✅ Response DTO
public class CollectionResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; } // ✅ Thay WorkspaceId bằng UserId
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int RequestsCount { get; set; }
}

// ✅ Create DTO
public class CreateCollectionDto
{
    public int UserId { get; set; } // ✅ Bắt buộc có UserId
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

// ✅ Update DTO
public class UpdateCollectionDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}