using AutoApiTester.App.Repositories;
using AutoApiTester.DTOs.SQLDto;
using AutoApiTester.Models;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

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
        public async Task<SQLConnectionDBEntity> SaveAsync(SQLConnectionDBEntity dto, int userId)
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
        public async Task<List<SQLConnectionDBEntity>> GetByUserIdAsync(int userId)
        {
            return await _repository.GetByUserIdAsync(userId);
        }

        // ✅ Lấy chi tiết theo Id
        public async Task<SQLConnectionDBEntity> GetByIdAsync(int id, int userId)
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



        private async Task<List<object>> ExecuteSelectAsync(SqlCommand command)
        {
            var results = new List<object>();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    if (reader.FieldCount == 1)
                    {
                        // ✅ 1 cột → Thêm value trực tiếp
                        var value = reader.IsDBNull(0) ? null : reader.GetValue(0);
                        results.Add(value);
                    }
                    else
                    {
                        // ✅ Nhiều cột → Thêm array of values
                        var row = new List<object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            row.Add(value);
                        }
                        results.Add(row);
                    }
                }
            }

            return results;
        }

        public async Task<SQLQueryResponse> ExecuteQueryAsync(string connectionStringOrId, string query, int userId, int timeout = 30)
        {
            var stopwatch = Stopwatch.StartNew();
            string connectionString = connectionStringOrId;

            try
            {
                // Nếu là ID thì load từ DB
                if (int.TryParse(connectionStringOrId, out var connectionId))
                {
                    var connectionEntity = await GetByIdAsync(connectionId, userId);

                    if (connectionEntity == null)
                        throw new ArgumentException("Connection not found or unauthorized.");

                    connectionString = connectionEntity.ConnectString;
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new ArgumentException("Connection string is required");

                if (string.IsNullOrWhiteSpace(query))
                    throw new ArgumentException("Query is required");

                // Normalize connection string
                connectionString = connectionString.Trim().TrimEnd(';');

                // Detect query type
                var queryType = DetectQueryType(query);

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = query;
                command.CommandTimeout = timeout;
                command.CommandType = CommandType.Text;

                if (queryType == QueryType.Select)
                {
                    // ✅ Trả về List<object>
                    var data = await ExecuteSelectAsync(command);
                    stopwatch.Stop();

                    return new SQLQueryResponse
                    {
                        Success = true,
                        Message = $"Query executed successfully. {data.Count} row(s) returned.",
                        Data = data,
                         RowsAffected = data.Count,
                        ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds
                    };
                }
                else
                {
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    stopwatch.Stop();

                    return new SQLQueryResponse
                    {
                        Success = true,
                        Message = $"Query executed successfully. {rowsAffected} row(s) affected.",
                        RowsAffected = rowsAffected,
                         ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds
                    };
                }
            }
            catch (SqlException ex)
            {
                stopwatch.Stop();
                return new SQLQueryResponse
                {
                    Success = false,
                    Message = $"SQL Error: {ex.Message}",
                    ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new SQLQueryResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds
                };
            }
        }
        private QueryType DetectQueryType(string query)
        {
            var trimmedQuery = query.Trim().ToUpper();

            if (trimmedQuery.StartsWith("SELECT") ||
                trimmedQuery.StartsWith("WITH") ||
                trimmedQuery.StartsWith("SHOW") ||
                trimmedQuery.StartsWith("DESCRIBE") ||
                trimmedQuery.StartsWith("EXPLAIN"))
            {
                return QueryType.Select;
            }

            return QueryType.NonQuery;
        }

        private enum QueryType
        {
            Select,
            NonQuery
        }

    }
}