using AutoApiTester.Models.DTOs;

namespace AutoApiTester.App.Services
{
    public interface IWorkspaceService
    {
        Task<IEnumerable<WorkspaceResponseDto>> GetUserWorkspacesAsync(int userId);
        Task<WorkspaceResponseDto?> GetByIdAsync(int id);
        Task<WorkspaceResponseDto> CreateAsync(CreateWorkspaceDto dto, int userId);
        Task<WorkspaceResponseDto?> UpdateAsync(int id, UpdateWorkspaceDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
