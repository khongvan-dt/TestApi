using AutoApiTester.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoApiTester.App.Repositories
{
    public interface ISQLConnectionDBRepository
    {
        Task<SQLConnectionDBEntity> SaveAsync(SQLConnectionDBEntity entity, int userId);
        Task<List<SQLConnectionDBEntity>> GetByUserIdAsync(int userId);
        Task<SQLConnectionDBEntity> GetByIdAsync(int id, int userId);
        Task<bool> DeleteAsync(int id, int userId);
    }
}