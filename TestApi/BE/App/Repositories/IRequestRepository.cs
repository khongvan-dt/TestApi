using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;

namespace AutoApiTester.App.Repositories
{
    public interface IRequestRepository : IRepository<Request>
    {
        Task<IEnumerable<RequestResponseDto>> GetByCollectionIdAsync(int collectionId);
        Task<RequestResponseDto?> GetByIdAsync(int id);
        Task<RequestResponseDto> CreateAsync(CreateRequestDto dto);
        Task<RequestResponseDto?> UpdateAsync(int id, UpdateRequestDto dto);
        Task<bool> DeleteAsync(int id);
        Task UpdateTestDataContentAsync(UpdateTestDataRequestDto dto);
    }
}
