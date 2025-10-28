//using AutoApiTester.App.Repositories;
//using AutoApiTester.App.Services;
//using AutoApiTester.Models.DTOs;

//namespace AutoApiTester.Services
//{
//    public class EnvironmentService : IEnvironmentService
//    {
//        private readonly IEnvironmentRepository _environmentRepo;

//        public EnvironmentService(IEnvironmentRepository environmentRepo)
//        {
//            _environmentRepo = environmentRepo;
//        }

//        public async Task<IEnumerable<EnvironmentResponseDto>> GetByWorkspaceIdAsync(int workspaceId)
//        {
//            return await _environmentRepo.GetByWorkspaceIdAsync(workspaceId);
//        }

//        public async Task<EnvironmentResponseDto?> GetByIdAsync(int id)
//        {
//            return await _environmentRepo.GetByIdAsync(id);
//        }

//        public async Task<EnvironmentResponseDto?> GetActiveByWorkspaceIdAsync(int workspaceId)
//        {
//            return await _environmentRepo.GetActiveByWorkspaceIdAsync(workspaceId);
//        }

//        public async Task<EnvironmentResponseDto> CreateAsync(CreateEnvironmentDto dto)
//        {
//            return await _environmentRepo.CreateAsync(dto);
//        }

//        public async Task<EnvironmentResponseDto?> UpdateAsync(int id, UpdateEnvironmentDto dto)
//        {
//            return await _environmentRepo.UpdateAsync(id, dto);
//        }

//        public async Task<bool> DeleteAsync(int id)
//        {
//            return await _environmentRepo.DeleteAsync(id);
//        }

//        public async Task<bool> SetActiveAsync(int id, int workspaceId)
//        {
//            return await _environmentRepo.SetActiveAsync(id, workspaceId);
//        }
//    }
//}
