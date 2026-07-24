using Dashboard.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<CardTask> CardTasks => Set<CardTask>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("DEPARTMENTS");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).HasMaxLength(120).IsRequired();
            entity.HasIndex(d => d.Name).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("USERS");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name).HasMaxLength(120).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(u => u.Role).HasMaxLength(60).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Card>(entity =>
        {
            entity.ToTable("CARDS");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Title).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Description).HasMaxLength(4000);
            entity.Property(c => c.Status).HasConversion<int>();
            entity.Property(c => c.Scope).HasConversion<int>();
            entity.Property(c => c.DevOpsUrl).HasMaxLength(1000);
            entity.Property(c => c.CreatedAt).HasPrecision(3);
            entity.Property(c => c.UpdatedAt).HasPrecision(3);
            entity.HasIndex(c => new { c.Status, c.SequenceOrder });
            entity.HasOne(c => c.Owner)
                .WithMany(u => u.OwnedCards)
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(c => c.Department)
                .WithMany(d => d.Cards)
                .HasForeignKey(c => c.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(c => c.Tasks)
                .WithOne(t => t.Card)
                .HasForeignKey(t => t.CardId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CardTask>(entity =>
        {
            entity.ToTable("CARD_TASKS");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).HasMaxLength(240).IsRequired();
            entity.Property(t => t.DevOpsUrl).HasMaxLength(1000);
            entity.Property(t => t.CreatedAt).HasPrecision(3);
            entity.Property(t => t.UpdatedAt).HasPrecision(3);
            entity.HasIndex(t => new { t.CardId, t.SequenceOrder });
            entity.HasOne(t => t.Assignee)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("REFRESH_TOKENS");
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.Token).HasMaxLength(512).IsRequired();
            entity.Property(rt => rt.ExpiresAt).IsRequired();
            entity.Property(rt => rt.CreatedAt).HasPrecision(3).IsRequired();
            entity.Property(rt => rt.IsRevoked).IsRequired();
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var engineeringId = 1;
        var productId = 2;
        var ownerId = 1;
        var assigneeId = 2;
        var personalCardId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var orgCardId = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var createdAt = new DateTime(2026, 7, 22, 8, 0, 0, DateTimeKind.Unspecified);

        modelBuilder.Entity<Department>().HasData(
            new Department { Id = engineeringId, Name = "Engineering" },
            new Department { Id = productId, Name = "Product" });

        // BCrypt hash of the seed password "Passw0rd!" for local/dev login testing.
        const string seedPasswordHash = "$2b$10$ucv6PQN2oV/TVOsyYpXKhOrPNJd4Pq4MDGdm6VzK4tHfS6kVUkAza";

        modelBuilder.Entity<User>().HasData(
            new User { Id = ownerId, Name = "Alex Owner", Email = "alex@example.com", PasswordHash = seedPasswordHash, DepartmentId = engineeringId, Role = "Owner" },
            new User { Id = assigneeId, Name = "Sam Assignee", Email = "sam@example.com", PasswordHash = seedPasswordHash, DepartmentId = engineeringId, Role = "Member" });

        modelBuilder.Entity<Card>().HasData(
            new Card
            {
                Id = personalCardId,
                Title = "Draft personal dashboard card",
                Description = "Seed card for personal board validation.",
                Status = CardStatus.Plan,
                Scope = CardScope.Personal,
                OwnerId = ownerId,
                DepartmentId = null,
                DueDate = new DateOnly(2026, 8, 15),
                SequenceOrder = 100,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new Card
            {
                Id = orgCardId,
                Title = "Coordinate organization workflow",
                Description = "Seed card for organization board validation.",
                Status = CardStatus.ToDo,
                Scope = CardScope.Organization,
                OwnerId = ownerId,
                DepartmentId = engineeringId,
                DueDate = new DateOnly(2026, 8, 20),
                SequenceOrder = 100,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            });

        modelBuilder.Entity<CardTask>().HasData(
            new CardTask
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                CardId = personalCardId,
                Title = "Confirm API model",
                IsCompleted = false,
                AssigneeId = ownerId,
                SequenceOrder = 100,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new CardTask
            {
                Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                CardId = orgCardId,
                Title = "Review task permissions",
                IsCompleted = false,
                AssigneeId = assigneeId,
                SequenceOrder = 100,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            });
    }
}
