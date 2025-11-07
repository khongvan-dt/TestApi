using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoApiTester.Models
{
    [Table("JobApiTestHistory")]
    public class JobApiTestHistoryEntity
    {
        [Key]
         public int Id { get; set; }

        [ForeignKey(nameof(ApiTestCase))]
        public int ApiTestCaseId { get; set; }

        public DateTime RunAt { get; set; } = DateTime.UtcNow;

        public string RequestPayload { get; set; }

        public string ResponseData { get; set; }

        public int StatusCode { get; set; }

        public bool IsSuccess { get; set; }

        public int DurationMs { get; set; }

        [MaxLength(1000)]
        public string ErrorMessage { get; set; }

        public JobApiTestCaseEntity ApiTestCase { get; set; }
    }
}
