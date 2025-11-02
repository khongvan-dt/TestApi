using AutoApiTester.Data;
using AutoApiTester.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoApiTester.App.Repositories
{
    public class SQLConnectionDBRepository : ISQLConnectionDBRepository
    {
        private readonly ApplicationDbContext _context;

        public SQLConnectionDBRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Save (Insert hoặc Update)
        public async Task<SQLConnectionDB> SaveAsync(SQLConnectionDB entity,int userId)
        {
            try {
                if (entity.Id > 0)
                {
                    // UPDATE
                    var existing = await _context.SQLConnectionDBs
                        .FirstOrDefaultAsync(x => x.Id == entity.Id && x.UserId == userId);

                    if (existing == null)
                        throw new Exception($"SQL Connection with ID {entity.Id} not found or access denied");

                    existing.Name = entity.Name;
                    existing.ConnectString = entity.ConnectString;
                    existing.IsActive = entity.IsActive;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UserId=userId;
                    _context.SQLConnectionDBs.Update(existing);
                }
                else
                {
                    // INSERT
                    entity.CreatedAt = DateTime.UtcNow;
                    entity.UpdatedAt = null;
                    entity.DeleteAt = null;
                    entity.UserId=userId;
                    await _context.SQLConnectionDBs.AddAsync(entity);
                }

                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                throw new Exception(errorMessage);
            }

        }

        // ✅ Lấy danh sách theo UserId
        public async Task<List<SQLConnectionDB>> GetByUserIdAsync(int userId)
        {
            return await _context.SQLConnectionDBs
                .Where(x => x.UserId == userId && x.DeleteAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        // ✅ Lấy chi tiết theo Id và UserId
        public async Task<SQLConnectionDB> GetByIdAsync(int id, int userId)
        {
            return await _context.SQLConnectionDBs
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId && x.DeleteAt == null);
        }

        // ✅ Soft Delete
        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var entity = await _context.SQLConnectionDBs
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId && x.DeleteAt == null);

            if (entity == null)
                return false;

            entity.DeleteAt = DateTime.UtcNow;
            entity.IsActive = false;

            _context.SQLConnectionDBs.Update(entity);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}