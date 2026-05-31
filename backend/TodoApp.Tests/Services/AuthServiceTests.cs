using FluentAssertions;
using Moq;
using Xunit;
using TodoApp.Core.DTOs;
using TodoApp.Core.Entities;
using TodoApp.Core.Interfaces;
using TodoApp.Services;

namespace TodoApp.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<ITokenService> _tokens = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _tokens.Setup(t => t.GenerateToken(It.IsAny<User>()))
               .Returns(("jwt-token", DateTime.UtcNow.AddHours(1)));
        _sut = new AuthService(_users.Object, _tokens.Object);
    }

    [Fact(DisplayName = "Реєстрація: новий email → повертає токен та користувача")]
    public async Task Реєстрація_НовийEmail_ПовертаєТокенТаКористувача()
    {
        var dto = new RegisterDto { Email = "new@user.com", UserName = "user", Password = "pass1234" };
        _users.Setup(r => r.EmailExistsAsync(dto.Email)).ReturnsAsync(false);
        _users.Setup(r => r.AddAsync(It.IsAny<User>()))
              .ReturnsAsync((User u) => { u.Id = 1; return u; });

        var result = await _sut.RegisterAsync(dto);

        result.Token.Should().Be("jwt-token");
        result.User.Email.Should().Be("new@user.com");
        result.User.UserName.Should().Be("user");
        _users.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Email == "new@user.com" && u.PasswordHash != "pass1234")), Times.Once);
    }

    [Fact(DisplayName = "Реєстрація: зайнятий email → кидає виняток")]
    public async Task Реєстрація_ЗайнятийEmail_КидаєВиняток()
    {
        var dto = new RegisterDto { Email = "taken@user.com", UserName = "u", Password = "pass1234" };
        _users.Setup(r => r.EmailExistsAsync(dto.Email)).ReturnsAsync(true);

        var act = () => _sut.RegisterAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*вже існує*");
    }

    [Fact(DisplayName = "Вхід: правильні дані → повертає токен")]
    public async Task Вхід_ПравильніДані_ПовертаєТокен()
    {
        var password = "secret123";
        var user = new User
        {
            Id = 5,
            Email = "ok@user.com",
            UserName = "ok",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };
        _users.Setup(r => r.GetByEmailAsync("ok@user.com")).ReturnsAsync(user);

        var result = await _sut.LoginAsync(new LoginDto { Email = "ok@user.com", Password = password });

        result.Token.Should().Be("jwt-token");
        result.User.Id.Should().Be(5);
    }

    [Fact(DisplayName = "Вхід: користувача не знайдено → помилка авторизації")]
    public async Task Вхід_КористувачаНеЗнайдено_КидаєВинятокБезДозволу()
    {
        _users.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var act = () => _sut.LoginAsync(new LoginDto { Email = "nope@u.com", Password = "x" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact(DisplayName = "Вхід: невірний пароль → помилка авторизації")]
    public async Task Вхід_НевірнийПароль_КидаєВинятокБезДозволу()
    {
        var user = new User
        {
            Email = "u@u.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("правильнийпароль")
        };
        _users.Setup(r => r.GetByEmailAsync("u@u.com")).ReturnsAsync(user);

        var act = () => _sut.LoginAsync(new LoginDto { Email = "u@u.com", Password = "невірнийпароль" });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
