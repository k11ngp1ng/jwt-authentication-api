using JwtAuthenticationApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthenticationApi.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);

            entity.Property(user => user.Username)
                .HasMaxLength(50)
                .UseCollation("NOCASE")
                .IsRequired();

            entity.Property(user => user.Email)
                .HasMaxLength(254)
                .UseCollation("NOCASE")
                .IsRequired();

            entity.Property(user => user.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(user => user.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(user => user.CreatedAt)
                .IsRequired();

            entity.HasIndex(user => user.Username)
                .IsUnique();

            entity.HasIndex(user => user.Email)
                .IsUnique();
        });
    }
}
