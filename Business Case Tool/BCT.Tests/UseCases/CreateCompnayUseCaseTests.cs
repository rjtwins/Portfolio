using BCT.Application.EventManagement.Events;
using BCT.Application.EventManagement.Notifiers;
using BCT.Application.ServiceInterfaces;
using BCT.Application.UseCases.Commands;
using BCT.Domain.Entities;
using Moq;

namespace BCT.Tests.UseCases;
public class CreateCompanyUseCaseTests
{
    private readonly Mock<ICompanyRepository> _companyRepositoryMock;
    private readonly Mock<NewCompanyNotifier> _newCompanyNotifierMock;
    private readonly CreateCompanyUseCase _useCase;

    public CreateCompanyUseCaseTests()
    {

        _companyRepositoryMock = new Mock<ICompanyRepository>();
        _newCompanyNotifierMock = new Mock<NewCompanyNotifier>();
        _useCase = new CreateCompanyUseCase(
            _companyRepositoryMock.Object,
            _newCompanyNotifierMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateCompanyAndAddUser()
    {
        // Arrange
        var creator = new User { Id = 1, Name = "Creator", AuthId = "auth0|creator" };
        var companyName = "Test Company";
        var company = new Company { Id = 1, Name = companyName, CreatorId = creator.Id };

        _companyRepositoryMock
            .Setup(repo => repo.Add(It.IsAny<Company>()))
            .ReturnsAsync(company);

        // Act
        var result = await _useCase.ExecuteAsync(creator, companyName, "Test User");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(companyName, result.Name);
        Assert.Equal(creator.Id, result.CreatorId);

        _companyRepositoryMock.Verify(repo => repo.Add(It.IsAny<Company>()), Times.Once);
        _companyRepositoryMock.Verify(repo => repo.AddUserToCompany(creator, company), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAddUserToCompany()
    {
        // Arrange
        var creator = new User { Id = 1, Name = "Creator", AuthId = "auth0|creator" };
        var companyName = "Test Company";
        var company = new Company { Id = 1, Name = companyName, CreatorId = creator.Id };

        _companyRepositoryMock
            .Setup(repo => repo.Add(It.IsAny<Company>()))
            .ReturnsAsync(company);

        // Act
        var result = await _useCase.ExecuteAsync(creator, companyName, "Test User");

        // Assert
        _companyRepositoryMock.Verify(repo => repo.AddUserToCompany(creator, company), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotifyNewCompany()
    {
        // Arrange
        var creator = new User { Id = 1, Name = "Creator", AuthId = "auth0|creator" };
        var companyName = "Test Company";
        var company = new Company { Id = 1, Name = companyName, CreatorId = creator.Id };

        _companyRepositoryMock
            .Setup(repo => repo.Add(It.IsAny<Company>()))
            .ReturnsAsync(company);

        // Act
        var result = await _useCase.ExecuteAsync(creator, companyName, "Test User");

        // Assert
        _newCompanyNotifierMock.Verify(notifier => notifier.Notify(It.IsAny<NewCompanyEvent>()), Times.Once);
    }
}