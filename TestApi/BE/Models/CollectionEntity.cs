namespace AutoApiTester.Models;

public class CollectionEntity
{
    public int Id { get; set; }
    public int UserId { get; set; } // ✅ Trực tiếp thuộc User
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public UserEntity? User { get; set; }
    public ICollection<RequestEntity> Requests { get; set; } = new List<RequestEntity>();
}