using System.ComponentModel.DataAnnotations;

namespace AutoApiTester.Models.DTOs;

public class CreateWorkspaceDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class UpdateWorkspaceDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class WorkspaceResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CollectionsCount { get; set; }
    public int EnvironmentsCount { get; set; }
}