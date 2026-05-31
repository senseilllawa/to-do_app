using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Xunit;
using Microsoft.Extensions.Configuration;
using TodoApp.Core.Entities;
using TodoApp.Services;

namespace TodoApp.Tests.Services;

public class TokenServiceTests
{
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsAVeryLongTestingSecretKeyForUnitTests_AtLeast32Chars",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiresHours"] = "1"
            })
            .Build();
        _sut = new TokenService(config);
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwtWithExpectedClaims()
    {
        var user = new User { Id = 123, Email = "test@user.com", UserName = "tester" };

        var (token, expires) = _sut.GenerateToken(user);

        token.Should().NotBeNullOrWhiteSpace();
        expires.Should().BeAfter(DateTime.UtcNow);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Issuer.Should().Be("TestIssuer");
        jwt.Audiences.Should().Contain("TestAudience");
        jwt.Claims.Should().Contain(c => c.Type == "sub" && c.Value == "123");
        jwt.Claims.Should().Contain(c => c.Value == "test@user.com");
        jwt.Claims.Should().Contain(c => c.Type == "userName" && c.Value == "tester");
    }

    [Fact]
    public void GenerateToken_ExpiresInConfiguredHours()
    {
        var user = new User { Id = 1, Email = "x@x.com", UserName = "x" };

        var (_, expires) = _sut.GenerateToken(user);

        expires.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromMinutes(1));
    }
}
