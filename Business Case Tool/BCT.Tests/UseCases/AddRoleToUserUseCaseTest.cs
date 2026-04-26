using BCT.Application.ServiceInterfaces;
using BCT.Domain.Entities;
using BCT.Application.UseCases.Commands;
using Moq;

namespace BCT.Tests.UseCases;

public class AddRoleToUserUseCaseTest
{
	private readonly Mock<IAuthManagementService> _authManagementServiceMock;
	private readonly AddRoleToUserUseCase _useCase;

	public AddRoleToUserUseCaseTest()
	{
		_authManagementServiceMock = new Mock<IAuthManagementService>();
		_useCase = new AddRoleToUserUseCase(_authManagementServiceMock.Object);
	}

	[Fact]
	public async Task ExecuteAsync_WithUserAndRole_CallsExecuteAsyncWithAuth0Ids()
	{
		// Arrange
		var user = new User { AuthId = "user-auth0-id" };
		var role = new Role { Name = "role-name", Auth0Id = "role-auth0-id" };

		// Act
		await _useCase.ExecuteAsync(user, role);

		// Assert
		_authManagementServiceMock.Verify(x => x.AddUserRole("user-auth0-id", "role-auth0-id"), Times.Once);
	}
	
	[Fact]
	public async Task ExecuteAsync_NullUser_ThrowsArgumentNullException()
	{
		// Arrange
		var role = new Role { Name = "role-name", Auth0Id = "role-auth0-id" };

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentNullException>(() => _useCase.ExecuteAsync(null, role));
	}
	
	[Fact]
	public async Task ExecuteAsync_NullRole_ThrowsArgumentNullException()
	{
		// Arrange
		User user = new User { AuthId = "user-auth0-id" };

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentNullException>(() => _useCase.ExecuteAsync(user, null));
	}
	
	[Fact]
	public async Task ExecuteAsync_EmptyAuth0Id_ThrowsArgumentException()
	{
		// Arrange
		User user = new User { AuthId = "" };
		Role role = new Role { Name = "role-name", Auth0Id = "role-auth0-id" };

		// Act & Assert
		await Assert.ThrowsAsync<ArgumentNullException>(() => _useCase.ExecuteAsync(user, role));
	}

	[Fact]
	public async Task ExecuteAsync_WithAuth0Ids_CallsAddUserRole()
	{
		// Act
		await _useCase.ExecuteAsync("user-auth0-id", "role-auth0-id");

		// Assert
		_authManagementServiceMock.Verify(x => x.AddUserRole("user-auth0-id", "role-auth0-id"), Times.Once);
	}
	
	[Fact]
	public async Task ExecuteAsync_WithAuth0Ids_ThrowsArgumentException()
	{
		// Act & Assert
		await Assert.ThrowsAsync<ArgumentNullException>(() => _useCase.ExecuteAsync("", ""));
	}
	
	[Fact]
	public async Task ExecuteAsync_WithAuth0Ids_ThrowsArgumentException2()
	{
		// Act & Assert
		await Assert.ThrowsAsync<ArgumentNullException>(() => _useCase.ExecuteAsync("", null));
	}
}
