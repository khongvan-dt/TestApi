using AutoApiTester.Models.DTOs;

namespace AutoApiTester.App.Services
{
    public interface ICollectionService
    {
         Task<CollectionResponseDto?> GetByIdAsync(int id);
        Task<CollectionResponseDto> CreateAsync(CreateCollectionDto dto);
        Task<CollectionResponseDto?> UpdateAsync(int id, UpdateCollectionDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
