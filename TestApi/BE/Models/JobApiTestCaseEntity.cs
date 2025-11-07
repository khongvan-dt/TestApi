using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoApiTester.Models
{
    [Table("JobApiTestCase")]
    public class JobApiTestCaseEntity
    {
        [Key]
         public int Id { get; set; }

        [ForeignKey(nameof(ApiTestSuite))]
        public int ApiTestSuiteId { get; set; }

        [MaxLength(200)]
        public string CaseName { get; set; }

        public string TestData { get; set; }

        public int ExpectedStatus { get; set; }

        public JobApiTestSuiteEntity ApiTestSuite { get; set; }

        public ICollection<JobApiTestHistoryEntity> Histories { get; set; }
    }
}
