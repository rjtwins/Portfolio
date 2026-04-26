using BCT.Application.ServiceInterfaces;
using BCT.Application.UseCases.Commands;
using BCT.Domain.Entities;
using Moq;

namespace BCT.Tests.UseCases;

public class RemoveRoleFromUserUseCaseTests
{
	[Fact]
	public async Task ExecuteAsync_ShouldRemoveRoleFromUser()
	{
		// Arrange
		var auth0ManagementApiMock = new Mock<IAuthManagementService>();
		var user = new User { Id = 1, AuthId = "auth0|123" };
		var role = new Role { Name = "", Auth0Id = "role|123" };

		auth0ManagementApiMock.Setup(x => x.RemoveUserRole(user.AuthId, role.Auth0Id)).Returns(Task.CompletedTask);

		var useCase = new RemoveRoleFromUserUseCase(auth0ManagementApiMock.Object);

		// Act
		await useCase.ExecuteAsync(user, role);

		// Assert
		auth0ManagementApiMock.Verify(x => x.RemoveUserRole(user.AuthId, role.Auth0Id), Times.Once);
	}

	[Fact]
	public async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenUserIsNull()
	{
		// Arrange
		var auth0ManagementApiMock = new Mock<IAuthManagementService>();
		var role = new Role { Name = "", Auth0Id = "role|123" };

		var useCase = new RemoveRoleFromUserUseCase(auth0ManagementApiMock.Object);

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentNullException>(() => useCase.ExecuteAsync(null, role));
	}

	[Fact]
	public async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenRoleIsNull()
	{
		// Arrange
		var auth0ManagementApiMock = new Mock<IAuthManagementService>();
		var user = new User { Id = 1, AuthId = "auth0|123" };

		var useCase = new RemoveRoleFromUserUseCase(auth0ManagementApiMock.Object);

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentNullException>(() => useCase.ExecuteAsync(user, null));
	}

	[Fact]
	public async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenUserAuthIdIsNullOrWhiteSpace()
	{
		// Arrange
		var auth0ManagementApiMock = new Mock<IAuthManagementService>();

		var useCase = new RemoveRoleFromUserUseCase(auth0ManagementApiMock.Object);

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentNullException>(() => useCase.ExecuteAsync("", "role|123"));
		await Assert.ThrowsAsync<ArgumentNullException>(() => useCase.ExecuteAsync(null, "role|123"));
	}

	[Fact]
	public async Task ExecuteAsync_ShouldThrowArgumentNullException_WhenRoleAuthIdIsNullOrEmpty()
	{
		// Arrange
		var auth0ManagementApiMock = new Mock<IAuthManagementService>();

		var useCase = new RemoveRoleFromUserUseCase(auth0ManagementApiMock.Object);

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentNullException>(() => useCase.ExecuteAsync("auth0|123", ""));
		await Assert.ThrowsAsync<ArgumentNullException>(() => useCase.ExecuteAsync("auth0|123", null));
	}
}