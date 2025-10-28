using AutoApiTester.App.Repositories;
using AutoApiTester.App.Services;
using AutoApiTester.Models.DTOs;

namespace AutoApiTester.Services
{
    public class CollectionService : ICollectionService
    {
        private readonly ICollectionRepository _collectionRepo;

        public CollectionService(ICollectionRepository collectionRepo)
        {
            _collectionRepo = collectionRepo;
        }

        //// ✅ Lấy danh sách Collection theo WorkspaceId
        //public async Task<IEnumerable<CollectionResponseDto>> GetByWorkspaceIdAsync(int workspaceId)
        //{
        //    return await _collectionRepo.GetByWorkspaceIdAsync(workspaceId);
        //}

        // ✅ Lấy 1 Collection theo Id
        public async Task<CollectionResponseDto?> GetByIdAsync(int id)
        {
            return await _collectionRepo.GetByIdAsync(id);
        }

        // ✅ Tạo mới Collection
        public async Task<CollectionResponseDto> CreateAsync(CreateCollectionDto dto)
        {
            return await _collectionRepo.CreateAsync(dto);
        }

        // ✅ Cập nhật Collection
        public async Task<CollectionResponseDto?> UpdateAsync(int id, UpdateCollectionDto dto)
        {
            return await _collectionRepo.UpdateAsync(id, dto);
        }

        // ✅ Xóa Collection
        public async Task<bool> DeleteAsync(int id)
        {
            return await _collectionRepo.DeleteAsync(id);
        }
    }
}
