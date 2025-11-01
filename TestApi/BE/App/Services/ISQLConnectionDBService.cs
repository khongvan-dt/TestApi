using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoApiTester.App.Services
{
    public interface ISQLConnectionDBService
    {
        Task<SQLConnectionDB> SaveAsync(SQLConnectionDB dto, int userId);
        Task<List<SQLConnectionDB>> GetByUserIdAsync(int userId);
        Task<SQLConnectionDB> GetByIdAsync(int id, int userId);
        Task<bool> DeleteAsync(int id, int userId);
        Task<bool> TestConnectionAsync(string connectionString);
    }
}