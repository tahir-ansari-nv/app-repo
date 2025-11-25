using System.Threading.Tasks;
using Moq;
using Xunit;

public class AuthenticationServiceTests
{
    [Fact]
    public async Task AuthenticateAsync_UserNotFound_ReturnsFalseAndNullToken()
    {
        // Arrange
        var usersRepoMock = new Mock<IUserRepository>();
        usersRepoMock.Setup(r => r.GetByNormalizedEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var securityMock = new Mock<ISecurityUtils>();
        securityMock.Setup(s => s.RandomDelayMs()).Returns(50);

        var jwtMock = new Mock<IJwtTokenGenerator>();

        var service = new AuthenticationService(
            usersRepoMock.Object,
            securityMock.Object,
            jwtMock.Object
        );

        // Act
        var (success, token) = await service.AuthenticateAsync("email", "pwd");

        // Assert
        Assert.False(success);
        Assert.Null(token);
        securityMock.Verify(s => s.RandomDelayMs(), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WrongPassword_ReturnsFalseAndNullToken()
    {
        // Arrange
        var user = new User
        {
            Id = System.Guid.NewGuid(),
            NormalizedEmail = "test@email.com",
            PasswordHash = "hashedPassword"
        };

        var usersRepoMock = new Mock<IUserRepository>();
        usersRepoMock.Setup(r => r.GetByNormalizedEmailAsync(user.NormalizedEmail))
            .ReturnsAsync(user);

        var securityMock = new Mock<ISecurityUtils>();
        securityMock.Setup(s => s.HashPassword("wrongpwd", user.PasswordHash)).Returns("wrongHash");
        securityMock.Setup(s => s.ConstantTimeEquals(user.PasswordHash, "wrongHash")).Returns(false);
        securityMock.Setup(s => s.RandomDelayMs()).Returns(55);

        var jwtMock = new Mock<IJwtTokenGenerator>();

        var service = new AuthenticationService(
            usersRepoMock.Object,
            securityMock.Object,
            jwtMock.Object
        );

        // Act
        var (success, token) = await service.AuthenticateAsync(user.NormalizedEmail, "wrongpwd");

        // Assert
        Assert.False(success);
        Assert.Null(token);
        usersRepoMock.Verify(r => r.GetByNormalizedEmailAsync(user.NormalizedEmail), Times.Once);
        securityMock.Verify(s => s.HashPassword("wrongpwd", user.PasswordHash), Times.Once);
        securityMock.Verify(s => s.ConstantTimeEquals(user.PasswordHash, "wrongHash"), Times.Once);
        securityMock.Verify(s => s.RandomDelayMs(), Times.Once);
        jwtMock.Verify(j => j.GenerateJwtToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_CorrectPassword_ReturnsTrueAndJwtToken()
    {
        // Arrange
        var user = new User
        {
            Id = System.Guid.NewGuid(),
            NormalizedEmail = "test@email.com",
            PasswordHash = "hashedPassword"
        };
        const string jwtToken = "jwt_token_string";

        var usersRepoMock = new Mock<IUserRepository>();
        usersRepoMock.Setup(r => r.GetByNormalizedEmailAsync(user.NormalizedEmail))
            .ReturnsAsync(user);

        var securityMock = new Mock<ISecurityUtils>();
        securityMock.Setup(s => s.HashPassword("thepwd", user.PasswordHash)).Returns("hashedPassword");
        securityMock.Setup(s => s.ConstantTimeEquals(user.PasswordHash, "hashedPassword")).Returns(true);

        var jwtMock = new Mock<IJwtTokenGenerator>();
        jwtMock.Setup(j => j.GenerateJwtToken(user)).Returns(jwtToken);

        var service = new AuthenticationService(
            usersRepoMock.Object,
            securityMock.Object,
            jwtMock.Object
        );

        // Act
        var (success, token) = await service.AuthenticateAsync(user.NormalizedEmail, "thepwd");

        // Assert
        Assert.True(success);
        Assert.Equal(jwtToken, token);
        usersRepoMock.Verify(r => r.GetByNormalizedEmailAsync(user.NormalizedEmail), Times.Once);
        securityMock.Verify(s => s.HashPassword("thepwd", user.PasswordHash), Times.Once);
        securityMock.Verify(s => s.ConstantTimeEquals(user.PasswordHash, "hashedPassword"), Times.Once);
        jwtMock.Verify(j => j.GenerateJwtToken(user), Times.Once);
        securityMock.Verify(s => s.RandomDelayMs(), Times.Never);
    }
}
