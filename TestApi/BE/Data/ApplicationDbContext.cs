using Microsoft.EntityFrameworkCore;
using AutoApiTester.Models;

namespace AutoApiTester.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<CollectionEntity> Collections { get; set; }
    public DbSet<RequestEntity> Requests { get; set; }
    public DbSet<ExecutionHistoryEntity> ExecutionHistories { get; set; }
    public DbSet<RequestHeaderEntity> RequestHeaders { get; set; }
    public DbSet<RequestParamEntity> RequestParams { get; set; }
    public DbSet<RequestBodyEntity> RequestBodies { get; set; }
    public DbSet<JobApiTestSuiteEntity> JobApiTestSuites { get; set; }
    public DbSet<JobApiTestCaseEntity> JobApiTestCases { get; set; }
    public DbSet<JobApiTestHistoryEntity> JobApiTestHistories { get; set; }
    public DbSet<JobScheduleApiTest> JobScheduleApiTests => Set<JobScheduleApiTest>();
    public DbSet<SQLConnectionDBEntity> SQLConnectionDBs => Set<SQLConnectionDBEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email);
        });

        // Collection - Thuộc trực tiếp về User
        modelBuilder.Entity<CollectionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
        });

        // Request
        modelBuilder.Entity<RequestEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Method).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Url).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.AuthType).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

            entity.HasOne(e => e.Collection)
                .WithMany(c => c.Requests)
                .HasForeignKey(e => e.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.CollectionId);
        });

        // ExecutionHistory
        modelBuilder.Entity<ExecutionHistoryEntity>(entity =>
        {
            entity.ToTable("ExecutionHistory");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Method).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Url).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.StatusText).HasMaxLength(100);
            entity.Property(e => e.ExecutedAt).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

            // User relationship
            entity.HasOne(e => e.User)
                .WithMany(u => u.ExecutionHistories)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Request relationship (nullable)
            entity.HasOne(e => e.Request)
                .WithMany()
                .HasForeignKey(e => e.RequestId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.RequestId);
            entity.HasIndex(e => e.ExecutedAt);
        });

        // RequestHeader
        modelBuilder.Entity<RequestHeaderEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).HasMaxLength(200);
            entity.Property(e => e.Value).HasMaxLength(1000);

            entity.HasOne(e => e.Request)
                .WithMany(r => r.RequestHeaders)
                .HasForeignKey(e => e.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.RequestId);
        });

        // RequestParam
        modelBuilder.Entity<RequestParamEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).HasMaxLength(200);
            entity.Property(e => e.Value).HasMaxLength(1000);

            entity.HasOne(e => e.Request)
                .WithMany(r => r.RequestParams)
                .HasForeignKey(e => e.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.RequestId);
        });

        // RequestBody
        modelBuilder.Entity<RequestBodyEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BodyType).HasMaxLength(50);
            entity.Property(e => e.Value).HasColumnType("nvarchar(max)");

            entity.HasOne(e => e.Request)
                .WithMany(r => r.RequestBodies)
                .HasForeignKey(e => e.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.RequestId);
        });

        // JobApiTestSuite
        modelBuilder.Entity<JobApiTestSuiteEntity>()
            .HasMany(s => s.TestCases)
            .WithOne(c => c.ApiTestSuite)
            .HasForeignKey(c => c.ApiTestSuiteId)
            .OnDelete(DeleteBehavior.Cascade);

        // JobApiTestCase
        modelBuilder.Entity<JobApiTestCaseEntity>()
            .HasMany(c => c.Histories)
            .WithOne(h => h.ApiTestCase)
            .HasForeignKey(h => h.ApiTestCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        // JobScheduleApiTest
        modelBuilder.Entity<JobScheduleApiTest>()
            .HasOne(j => j.User)
            .WithMany()
            .HasForeignKey(j => j.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SQLConnectionDBEntity>(entity =>
       {
           entity.ToTable("SQLConnectionDB");
           entity.HasKey(e => e.Id);

           entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
           entity.Property(e => e.ConnectString).HasMaxLength(1000).IsRequired();
           entity.Property(e => e.IsActive).HasDefaultValue(true);
           entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

           entity.HasOne(e => e.User)
          .WithMany(u => u.SQLConnectionDBs)
          .HasForeignKey(e => e.UserId)
          .OnDelete(DeleteBehavior.Restrict);

           entity.HasIndex(e => e.UserId);
       });


    }
}