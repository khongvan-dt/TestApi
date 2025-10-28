using AutoApiTester.Models.DTOs;

namespace AutoApiTester.App.Services
{
    public interface IRequestService
    {
        Task<IEnumerable<RequestResponseDto>> GetByCollectionIdAsync(int collectionId);
        Task<RequestResponseDto?> GetByIdAsync(int id);
        Task<RequestResponseDto> CreateAsync(CreateRequestDto dto);
        Task<RequestResponseDto?> UpdateAsync(int id, UpdateRequestDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
