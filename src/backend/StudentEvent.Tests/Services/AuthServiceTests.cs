using Moq;
using Microsoft.Extensions.Configuration;
using StudentEvent.Application.Services;
using StudentEvent.Application.DTOs;
using StudentEvent.Domain.Interfaces;
using StudentEvent.Domain.Entities;
using Xunit;

namespace StudentEvent.Tests.Services;

// Testes unitários para o serviço de autenticação (AuthService)
public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockConfig = new Mock<IConfiguration>();

        // Configuração Mockada para o JWT
        _mockConfig.Setup(c => c["Jwt:Key"]).Returns("7C06EF1F-27FA-4A66-A58B-7BDCA07D94F1");
        _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("StudentEventAPI");
        _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("StudentEventFrontEnd");

        _authService = new AuthService(_mockUserRepo.Object, _mockConfig.Object);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var email = "admin@teste.com";
        var password = "123";
        var user = new User { Id = Guid.NewGuid(), Email = email, PasswordHash = password };

        _mockUserRepo.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(user);

        var loginDto = new LoginRequestDto { Email = email, Password = password };

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Token));
        Assert.Equal(email, result.Email);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsInvalid()
    {
        // Arrange
        var email = "admin@teste.com";
        var user = new User { Id = Guid.NewGuid(), Email = email, PasswordHash = "senha_correta" };

        _mockUserRepo.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(user);

        var loginDto = new LoginRequestDto { Email = email, Password = "senha_errada" };

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.Null(result);
    }
}