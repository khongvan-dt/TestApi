using AutoApiTester.App.Repositories;
using AutoApiTester.Data;
using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AutoApiTester.Repositories;

public class CollectionRepository : Repository<Collection>, ICollectionRepository
{
    private readonly ApplicationDbContext _context;

    public CollectionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // ✅ Lấy danh sách collection theo UserId
    public async Task<IEnumerable<CollectionResponseDto>> GetByUserIdAsync(int userId)
    {
        var collections = await _context.Collections
            .Include(c => c.Requests)
            .Where(c => c.UserId == userId) // ✅ Filter theo UserId
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return collections.Select(c => new CollectionResponseDto
        {
            Id = c.Id,
            UserId = c.UserId,
            Name = c.Name,
            Description = c.Description,
            CreatedAt = c.CreatedAt,
            RequestsCount = c.Requests?.Count ?? 0
        });
    }

    // ✅ Lấy 1 collection theo Id (kèm requests)
    public async Task<CollectionResponseDto?> GetByIdAsync(int id)
    {
        var collection = await _context.Collections
            .Include(c => c.Requests)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (collection == null) return null;

        return new CollectionResponseDto
        {
            Id = collection.Id,
            UserId = collection.UserId,
            Name = collection.Name,
            Description = collection.Description,
            CreatedAt = collection.CreatedAt,
            RequestsCount = collection.Requests?.Count ?? 0
        };
    }

    // ✅ Tạo mới collection
    public async Task<CollectionResponseDto> CreateAsync(CreateCollectionDto dto)
    {
        var collection = new Collection
        {
            UserId = dto.UserId,
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Collections.AddAsync(collection);
        await _context.SaveChangesAsync();

        return new CollectionResponseDto
        {
            Id = collection.Id,
            UserId = collection.UserId,
            Name = collection.Name,
            Description = collection.Description,
            CreatedAt = collection.CreatedAt,
            RequestsCount = 0
        };
    }

    // ✅ Cập nhật collection
    public async Task<CollectionResponseDto?> UpdateAsync(int id, UpdateCollectionDto dto)
    {
        var collection = await _context.Collections.FindAsync(id);
        if (collection == null) return null;

        collection.Name = dto.Name;
        collection.Description = dto.Description;

        _context.Collections.Update(collection);
        await _context.SaveChangesAsync();

        return new CollectionResponseDto
        {
            Id = collection.Id,
            UserId = collection.UserId,
            Name = collection.Name,
            Description = collection.Description,
            CreatedAt = collection.CreatedAt,
            RequestsCount = await _context.Requests.CountAsync(r => r.CollectionId == id)
        };
    }

    // ✅ Xóa collection
    public async Task<bool> DeleteAsync(int id)
    {
        var collection = await _context.Collections.FindAsync(id);
        if (collection == null) return false;

        _context.Collections.Remove(collection);
        await _context.SaveChangesAsync();

        return true;
    }

    // ✅ Kiểm tra collection có thuộc về user không
    public async Task<bool> BelongsToUserAsync(int collectionId, int userId)
    {
        return await _context.Collections
            .AnyAsync(c => c.Id == collectionId && c.UserId == userId);
    }
}