using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoApiTester.Models
{
    [Table("JobScheduleApiTest")]
    public class JobScheduleApiTestEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }  

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string ScheduleType { get; set; } = "daily";

        public TimeSpan? RunAtTime { get; set; }
        public int? IntervalMinutes { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime? LastRunAt { get; set; }
        public DateTime? NextRunAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(UserId))]
        public virtual UserEntity User { get; set; } = null!;

        public virtual ICollection<JobApiTestSuiteEntity> JobApiTestSuites { get; set; } = new List<JobApiTestSuiteEntity>();
    }
}