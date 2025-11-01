using AutoApiTester.App.Repositories;
using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace AutoApiTester.App.Services
{
    public class SQLConnectionDBService : ISQLConnectionDBService
    {
        private readonly ISQLConnectionDBRepository _repository;

        public SQLConnectionDBService(ISQLConnectionDBRepository repository)
        {
            _repository = repository;
        }

        // ✅ Save (Insert hoặc Update)
        public async Task<SQLConnectionDB> SaveAsync(SQLConnectionDB dto, int userId)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required");

            if (string.IsNullOrWhiteSpace(dto.ConnectString))
                throw new ArgumentException("Connection string is required");

            if (dto.Name.Length > 200)
                throw new ArgumentException("Name must not exceed 200 characters");

            if (dto.ConnectString.Length > 1000)
                throw new ArgumentException("Connection string must not exceed 1000 characters");

           

            return await _repository.SaveAsync(dto,  userId);
        }

        // ✅ Lấy danh sách theo UserId
        public async Task<List<SQLConnectionDB>> GetByUserIdAsync(int userId)
        {
            return await _repository.GetByUserIdAsync(userId);
        }

        // ✅ Lấy chi tiết theo Id
        public async Task<SQLConnectionDB> GetByIdAsync(int id, int userId)
        {
            var entity = await _repository.GetByIdAsync(id, userId);

            if (entity == null)
                throw new Exception($"SQL Connection with ID {id} not found");

            return entity;
        }

        // ✅ Xóa
        public async Task<bool> DeleteAsync(int id, int userId)
        {
            return await _repository.DeleteAsync(id, userId);
        }

        // ✅ Test Connection String
        public async Task<bool> TestConnectionAsync(string connectionString)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}