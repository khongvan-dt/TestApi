using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;

namespace AutoApiTester.App.Repositories;

public interface IWorkspaceRepository : IRepository<Workspace>
{
    Task<IEnumerable<WorkspaceResponseDto>> GetUserWorkspacesAsync(int userId);
    Task<WorkspaceResponseDto?> GetByIdAsync(int id);
    Task<WorkspaceResponseDto> CreateAsync(CreateWorkspaceDto dto, int userId);
    Task<WorkspaceResponseDto?> UpdateAsync(int id, UpdateWorkspaceDto dto);
    Task<bool> DeleteAsync(int id);
}