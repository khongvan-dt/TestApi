using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoApiTester.Models;

[Table("Users")]
public class UserEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? FullName { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
     public virtual ICollection<ExecutionHistoryEntity> ExecutionHistories { get; set; } = new List<ExecutionHistoryEntity>();
    public virtual ICollection<JobScheduleApiTest> JobScheduleApiTests { get; set; } = new List<JobScheduleApiTest>();
    public virtual ICollection<SQLConnectionDBEntity> SQLConnectionDBs { get; set; } = new List<SQLConnectionDBEntity>();

}