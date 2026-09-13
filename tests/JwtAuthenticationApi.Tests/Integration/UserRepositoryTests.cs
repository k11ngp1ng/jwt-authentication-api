using JwtAuthenticationApi.Domain.Entities;
using JwtAuthenticationApi.Infrastructure.Persistence;
using JwtAuthenticationApi.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthenticationApi.Tests.Integration;

public sealed class UserRepositoryTests
{
    [Fact]
    public async Task AddAndGetByEmailAsync_ShouldPersistUser()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var repository = new UserRepository(dbContext);
        var user = new User("repository_user", "repo@example.com", "password-hash");

        await repository.AddAsync(user);
        var persistedUser = await repository.GetByEmailAsync("REPO@EXAMPLE.COM");

        Assert.NotNull(persistedUser);
        Assert.Equal(user.Id, persistedUser.Id);
    }
}
