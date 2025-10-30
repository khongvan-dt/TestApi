using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoApiTester.Models
{
    [Table("JobApiTestCase")]
    public class JobApiTestCase
    {
        [Key]
         public int Id { get; set; }

        [ForeignKey(nameof(ApiTestSuite))]
        public int ApiTestSuiteId { get; set; }

        [MaxLength(200)]
        public string CaseName { get; set; }

        public string TestData { get; set; }

        public int ExpectedStatus { get; set; }

        public JobApiTestSuite ApiTestSuite { get; set; }

        public ICollection<JobApiTestHistory> Histories { get; set; }
    }
}
