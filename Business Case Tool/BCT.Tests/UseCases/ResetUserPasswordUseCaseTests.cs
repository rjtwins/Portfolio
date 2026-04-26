using BCT.Application.ServiceInterfaces;
using BCT.Application.UseCases.Commands;
using BCT.Domain.Entities;
using Moq;

namespace BCT.Tests.UseCases;

public class ResetUserPasswordUseCaseTests
{
	[Fact]
	public async Task ExecuteAsync_ShouldResetUserPassword()
	{
		// Arrange
		var auth0ManagementApiMock = new Mock<IAuthManagementService>();
		var user = new User { Name = "", Id = 1, Email = "test@example.com", AuthId = "auth0|123" };

		auth0ManagementApiMock.Setup(x => x.ResetPasswordByEmail(user.Email)).Returns(Task.CompletedTask);

		var useCase = new ResetUserPasswordUseCase(auth0ManagementApiMock.Object);

		// Act
		await useCase.ExecuteAsync(user);

		// Assert
		auth0ManagementApiMock.Verify(x => x.ResetPasswordByEmail(user.Email), Times.Once);
	}

	[Fact]
	public async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenUserIsNull()
	{
		// Arrange
		var auth0ManagementApiMock = new Mock<IAuthManagementService>();

		var useCase = new ResetUserPasswordUseCase(auth0ManagementApiMock.Object);

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentNullException>(() => useCase.ExecuteAsync(null));
	}

	[Fact]
	public async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenUserEmailIsNull()
	{
		// Arrange
		var auth0ManagementApiMock = new Mock<IAuthManagementService>();
		var user = new User { Name = "", Id = 1, Email = null, AuthId = "auth0|123" };

		var useCase = new ResetUserPasswordUseCase(auth0ManagementApiMock.Object);

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentNullException>(() => useCase.ExecuteAsync(user));
	}
}