using JwtAuthenticationApi.Application.Exceptions;
using JwtAuthenticationApi.Application.Interfaces;
using JwtAuthenticationApi.Domain.Entities;
using JwtAuthenticationApi.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthenticationApi.Infrastructure.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .AnyAsync(user => user.Email == email, cancellationToken);
    }

    public Task<bool> UsernameExistsAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .AnyAsync(user => user.Username == username, cancellationToken);
    }

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        dbContext.Users.Add(user);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            throw new ResourceConflictException(
                "A user with the same email or username already exists.");
        }
    }
}
