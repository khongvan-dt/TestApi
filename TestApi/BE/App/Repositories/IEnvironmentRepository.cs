//using AutoApiTester.Models;
//using AutoApiTester.Models.DTOs;

//namespace AutoApiTester.App.Repositories
//{
//    public interface IEnvironmentRepository : IRepository<Environmentss>
//    {
//        Task<IEnumerable<EnvironmentResponseDto>> GetByWorkspaceIdAsync(int workspaceId);
//        Task<EnvironmentResponseDto?> GetByIdAsync(int id);
//        Task<EnvironmentResponseDto?> GetActiveByWorkspaceIdAsync(int workspaceId);
//        Task<EnvironmentResponseDto> CreateAsync(CreateEnvironmentDto dto);
//        Task<EnvironmentResponseDto?> UpdateAsync(int id, UpdateEnvironmentDto dto);
//        Task<bool> DeleteAsync(int id);
//        Task<bool> SetActiveAsync(int id, int workspaceId);
//    }
//}
