using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoApiTester.Models
{
    [Table("JobApiTestSuite")]
    public class JobApiTestSuiteEntity
    {
        [Key]
         public int Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Endpoint { get; set; }

        [MaxLength(10)]
        public string Method { get; set; }

        public string Headers { get; set; }  

        public string DataBase { get; set; }  

        [MaxLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [MaxLength(100)]
        public string UpdatedBy { get; set; }

        public ICollection<JobApiTestCaseEntity> TestCases { get; set; }
    }
}
