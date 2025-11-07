using AutoApiTester.DTOs.SQLDto;
using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoApiTester.App.Services
{
    public interface ISQLConnectionDBService
    {
        Task<SQLConnectionDBEntity> SaveAsync(SQLConnectionDBEntity dto, int userId);
        Task<List<SQLConnectionDBEntity>> GetByUserIdAsync(int userId);
        Task<SQLConnectionDBEntity> GetByIdAsync(int id, int userId);
        Task<bool> DeleteAsync(int id, int userId);
        Task<bool> TestConnectionAsync(string connectionString);
        Task<SQLQueryResponse> ExecuteQueryAsync(string connectionString, string query, int userId, int timeout = 30);
    }
}