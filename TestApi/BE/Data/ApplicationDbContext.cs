using Microsoft.EntityFrameworkCore;
using AutoApiTester.Models;

namespace AutoApiTester.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Collection> Collections { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<ExecutionHistory> ExecutionHistories { get; set; }
    public DbSet<RequestHeader> RequestHeaders { get; set; }
    public DbSet<RequestParam> RequestParams { get; set; }
    public DbSet<RequestBody> RequestBodies { get; set; }
    public DbSet<JobApiTestSuite> JobApiTestSuites { get; set; }
    public DbSet<JobApiTestCase> JobApiTestCases { get; set; }
    public DbSet<JobApiTestHistory> JobApiTestHistories { get; set; }
    public DbSet<JobScheduleApiTest> JobScheduleApiTests => Set<JobScheduleApiTest>();
    public DbSet<SQLConnectionDB> SQLConnectionDBs => Set<SQLConnectionDB>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
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
        modelBuilder.Entity<Collection>(entity =>
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
        modelBuilder.Entity<Request>(entity =>
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
        modelBuilder.Entity<ExecutionHistory>(entity =>
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
        modelBuilder.Entity<RequestHeader>(entity =>
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
        modelBuilder.Entity<RequestParam>(entity =>
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
        modelBuilder.Entity<RequestBody>(entity =>
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
        modelBuilder.Entity<JobApiTestSuite>()
            .HasMany(s => s.TestCases)
            .WithOne(c => c.ApiTestSuite)
            .HasForeignKey(c => c.ApiTestSuiteId)
            .OnDelete(DeleteBehavior.Cascade);

        // JobApiTestCase
        modelBuilder.Entity<JobApiTestCase>()
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

        modelBuilder.Entity<SQLConnectionDB>(entity =>
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