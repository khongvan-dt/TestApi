using AutoApiTester.App.Repositories;
using AutoApiTester.App.Services;
using AutoApiTester.Models.DTOs;

namespace AutoApiTester.Services;

public class RequestService : IRequestService
{
    private readonly IRequestRepository _requestRepo;

    public RequestService(IRequestRepository requestRepo)
    {
        _requestRepo = requestRepo;
    }

    // ✅ Lấy tất cả request theo collectionId
    public async Task<IEnumerable<RequestResponseDto>> GetByCollectionIdAsync(int collectionId)
    {
        return await _requestRepo.GetByCollectionIdAsync(collectionId);
    }

    // ✅ Lấy 1 request theo Id
    public async Task<RequestResponseDto?> GetByIdAsync(int id)
    {
        return await _requestRepo.GetByIdAsync(id);
    }

    // ✅ Tạo mới
    public async Task<RequestResponseDto> CreateAsync(CreateRequestDto dto)
    {
        return await _requestRepo.CreateAsync(dto);
    }

    // ✅ Cập nhật
    public async Task<RequestResponseDto?> UpdateAsync(int id, UpdateRequestDto dto)
    {
        return await _requestRepo.UpdateAsync(id, dto);
    }

    // ✅ Xóa
    public async Task<bool> DeleteAsync(int id)
    {
        return await _requestRepo.DeleteAsync(id);
    }
    // ✅ Cập nhật nội dung test data
    public async Task UpdateTestDataContentAsync(UpdateTestDataRequestDto dto)
    {
        await _requestRepo.UpdateTestDataContentAsync(dto);
    }
}
