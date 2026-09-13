using JwtAuthenticationApi.Infrastructure.Authentication;

namespace JwtAuthenticationApi.Tests.Unit;

public sealed class BcryptPasswordHasherTests
{
    [Fact]
    public void HashAndVerify_ShouldProtectAndValidatePassword()
    {
        const string password = "StrongPassword@123";
        var passwordHasher = new BcryptPasswordHasher();

        var passwordHash = passwordHasher.Hash(password);

        Assert.NotEqual(password, passwordHash);
        Assert.True(passwordHasher.Verify(password, passwordHash));
        Assert.False(passwordHasher.Verify("WrongPassword@123", passwordHash));
    }
}
