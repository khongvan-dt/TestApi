using AutoApiTester.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoApiTester.App.Repositories
{
    public interface ISQLConnectionDBRepository
    {
        Task<SQLConnectionDB> SaveAsync(SQLConnectionDB entity, int userId);
        Task<List<SQLConnectionDB>> GetByUserIdAsync(int userId);
        Task<SQLConnectionDB> GetByIdAsync(int id, int userId);
        Task<bool> DeleteAsync(int id, int userId);
    }
}