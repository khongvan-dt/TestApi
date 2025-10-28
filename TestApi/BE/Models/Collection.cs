namespace AutoApiTester.Models;

public class Collection
{
    public int Id { get; set; }
    public int UserId { get; set; } // ✅ Trực tiếp thuộc User
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public User? User { get; set; }
    public ICollection<Request> Requests { get; set; } = new List<Request>();
}