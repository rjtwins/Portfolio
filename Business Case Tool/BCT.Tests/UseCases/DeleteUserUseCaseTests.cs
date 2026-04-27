using BCT.Application.ServiceInterfaces;
using BCT.Application.UseCases.Commands;
using BCT.Domain.Entities;
using Moq;

namespace BCT.Tests.UseCases;

public class DeleteUserUseCaseTests
{
	[Fact]
	public async Task ExecuteAsync_ShouldDeleteUser()
	{
		// Arrange
		var auth0ManagementApiMock = new Mock<IAuthManagementService>();
		var userRepositoryMock = new Mock<IRepository<User>>();

		var user = new User { Id = 1, AuthId = "auth0|123" };

		auth0ManagementApiMock.Setup(x => x.DeleteUser(user.AuthId)).Returns(Task.CompletedTask);
		userRepositoryMock.Setup(x => x.Delete(user)).Returns(Task.CompletedTask);

		var useCase = new DeleteUserUseCase(auth0ManagementApiMock.Object, userRepositoryMock.Object);

		// Act
		await useCase.ExecuteAsync(user);

		// Assert
		auth0ManagementApiMock.Verify(x => x.DeleteUser(user.AuthId), Times.Once);
		userRepositoryMock.Verify(x => x.Delete(user), Times.Once);
	}
	
	[Fact]
	public async Task ExecuteAsync_ShouldDeleteUser_ShouldThrowException()
	{
		// Arrange
		var auth0ManagementApiMock = new Mock<IAuthManagementService>();
		var userRepositoryMock = new Mock<IRepository<User>>();

		var user = new User { Id = 1, AuthId = "auth0|123" };

		auth0ManagementApiMock.Setup(x => x.DeleteUser(user.AuthId)).Returns(Task.CompletedTask);
		userRepositoryMock.Setup(x => x.Delete(user)).Returns(Task.CompletedTask);

		var useCase = new DeleteUserUseCase(auth0ManagementApiMock.Object, userRepositoryMock.Object);

		// Act/assert
		await Assert.ThrowsAsync<ArgumentNullException>(() => useCase.ExecuteAsync(null));
	}
}
