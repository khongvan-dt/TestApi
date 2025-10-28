using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;

namespace AutoApiTester.App.Repositories;

public interface ICollectionRepository : IRepository<Collection>
{
    // ✅ Lấy collections theo UserId
    Task<List<CollectionResponseDto>> GetByUserIdAsync(int userId);

    // ✅ Lấy 1 collection theo Id
    Task<CollectionResponseDto?> GetByIdAsync(int id);

    // ✅ Tạo mới
    Task<CollectionResponseDto> CreateAsync(CreateCollectionDto dto);

    // ✅ Cập nhật
    Task<CollectionResponseDto?> UpdateAsync(int id, UpdateCollectionDto dto);

    // ✅ Xóa
    Task<bool> DeleteAsync(int id);

    // ✅ Kiểm tra collection có thuộc về user không
    Task<bool> BelongsToUserAsync(int collectionId, int userId);
}