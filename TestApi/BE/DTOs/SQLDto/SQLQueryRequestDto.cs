using System.ComponentModel.DataAnnotations;

namespace AutoApiTester.DTOs.SQLDto
{

    public class SQLQueryRequestDto
    {
        [Required]
        [MaxLength(1000)]
        public string ConnectionString { get; set; } = string.Empty;

        [Required]
        public string Query { get; set; } = string.Empty;

        public int? Timeout { get; set; } = 30; // seconds
    }

    public class SQLQueryResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

         public List<object>? Data { get; set; }

        public int RowsAffected { get; set; }
        public double ExecutionTimeMs { get; set; }
     }
}

