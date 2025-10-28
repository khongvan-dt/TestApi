using AutoApiTester.App.Repositories;
using AutoApiTester.App.Services;
using AutoApiTester.Models.DTOs;

namespace AutoApiTester.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly IWorkspaceRepository _workspaceRepo;

        public WorkspaceService(IWorkspaceRepository workspaceRepo)
        {
            _workspaceRepo = workspaceRepo;
        }

        // ✅ Lấy danh sách workspace của người dùng
        public async Task<IEnumerable<WorkspaceResponseDto>> GetUserWorkspacesAsync(int userId)
        {
            return await _workspaceRepo.GetUserWorkspacesAsync(userId);
        }

        // ✅ Lấy chi tiết workspace
        public async Task<WorkspaceResponseDto?> GetByIdAsync(int id)
        {
            return await _workspaceRepo.GetByIdAsync(id);
        }

        // ✅ Tạo workspace mới
        public async Task<WorkspaceResponseDto> CreateAsync(CreateWorkspaceDto dto, int userId)
        {
            return await _workspaceRepo.CreateAsync(dto, userId);
        }

        // ✅ Cập nhật workspace
        public async Task<WorkspaceResponseDto?> UpdateAsync(int id, UpdateWorkspaceDto dto)
        {
            return await _workspaceRepo.UpdateAsync(id, dto);
        }

        // ✅ Xóa workspace
        public async Task<bool> DeleteAsync(int id)
        {
            return await _workspaceRepo.DeleteAsync(id);
        }
    }
}
