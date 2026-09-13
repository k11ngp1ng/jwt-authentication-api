using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using JwtAuthenticationApi.Application.DTOs.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JwtAuthenticationApi.Tests.Integration;

public sealed class AuthenticationFlowTests
{
    [Fact]
    public async Task Api_ShouldRegisterAuthenticateAndEnforceRoles()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"jwt-auth-tests-{Guid.NewGuid():N}.db");
        WebApplicationFactory<Program>? factory = null;
        HttpClient? client = null;

        try
        {
            factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Development");
                    builder.ConfigureLogging(logging => logging.ClearProviders());
                    builder.ConfigureAppConfiguration((_, configuration) =>
                    {
                        configuration.AddInMemoryCollection(
                            new Dictionary<string, string?>
                            {
                                ["ConnectionStrings:DefaultConnection"] =
                                    $"Data Source={databasePath};Pooling=False",
                                ["Jwt:Issuer"] = "integration-tests",
                                ["Jwt:Audience"] = "integration-tests-client",
                                ["Jwt:SecretKey"] = new string('i', 64),
                                ["Jwt:ExpirationMinutes"] = "30"
                            });
                    });
                });
            client = factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });

            var anonymousResponse = await client.GetAsync("/api/protected/user");
            Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

            var swaggerResponse = await client.GetAsync("/swagger/v1/swagger.json");
            Assert.Equal(HttpStatusCode.OK, swaggerResponse.StatusCode);
            Assert.Contains(
                "\"securitySchemes\"",
                await swaggerResponse.Content.ReadAsStringAsync());

            var registerResponse = await client.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest
                {
                    Username = "integration_user",
                    Email = "integration@example.com",
                    Password = "StrongPassword@123"
                });
            Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

            var authentication = await registerResponse.Content
                .ReadFromJsonAsync<AuthResponse>();
            Assert.NotNull(authentication);

            var duplicateResponse = await client.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest
                {
                    Username = "integration_user",
                    Email = "integration@example.com",
                    Password = "StrongPassword@123"
                });
            Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authentication.AccessToken);

            var userResponse = await client.GetAsync("/api/protected/user");
            Assert.Equal(HttpStatusCode.OK, userResponse.StatusCode);

            var adminResponse = await client.GetAsync("/api/protected/admin");
            Assert.Equal(HttpStatusCode.Forbidden, adminResponse.StatusCode);

            var loginResponse = await client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest
                {
                    Email = "integration@example.com",
                    Password = "StrongPassword@123"
                });
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var invalidLoginResponse = await client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest
                {
                    Email = "integration@example.com",
                    Password = "WrongPassword@123"
                });
            Assert.Equal(HttpStatusCode.Unauthorized, invalidLoginResponse.StatusCode);
        }
        finally
        {
            client?.Dispose();

            if (factory is not null)
            {
                await factory.DisposeAsync();
            }

            File.Delete(databasePath);
        }
    }
}
