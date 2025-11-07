using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoApiTester.Models;

[Table("Environments")]
public class EnvironmentEntity
{
    [Key]
    public int Id { get; set; } 

    [Required]
    public int WorkspaceId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Variables { get; set; } // JSON

    public bool IsActive { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

   
}