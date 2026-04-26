using BCT.Domain.Entities;
using BCT.EF;
using BCT.EF.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BCT.Tests.Repositories;

public class UserRepositoryTests
{
    public UserRepositoryTests()
    {
        var contextFactory = new DbContextFactory<ApplicationDbContext>(GetInMemoryOptions());
        using var context = contextFactory.CreateDbContext();

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    private DbContextOptions<ApplicationDbContext> GetInMemoryOptions()
	{
		return new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(databaseName: "TestDatabase")
			.Options;
	}

	[Fact]
	public async Task AddUser_ShouldAddUser()
    {
        //Arrange
        var options = GetInMemoryOptions();
		var contextFactory = new DbContextFactory<ApplicationDbContext>(options);
		var repository = new Repository<User>(contextFactory);
		var user = new User { Id = 1, Name = "Test User", AuthId = "auth0|123" };

        //Act
        var result = await repository.Add(user);

        //Assert
		Assert.NotNull(result);
		Assert.Equal(user.Id, result.Id);
		Assert.NotEmpty(await repository.GetAll());
		Assert.Equal(user.Id, (await repository.Get(user.Id)).Id);
	}

	[Fact]
	public async Task DeleteUser_ShouldDeleteUser()
	{
        //Arrange
        var options = GetInMemoryOptions();
		var contextFactory = new DbContextFactory<ApplicationDbContext>(options);
		var repository = new Repository<User>(contextFactory);

		var user = new User { Name = "Test User", AuthId = "auth0|123" };
        user = await repository.Add(user);

        //Act
        await repository.Delete(user);

        //Assert
		var result = await repository.Get(user.Id);
		Assert.Null(result);
	}

    [Fact]
    public async Task GetAll_ShouldGetAllUsers()
    {
        //Arrange
        var options = GetInMemoryOptions();
        var contextFactory = new DbContextFactory<ApplicationDbContext>(options);
        var repository = new Repository<User>(contextFactory);

        var user = new User { Name = "Test User", AuthId = "auth0|123" };
        var user2 = new User { Name = "Test User", AuthId = "auth0|1234" };
        await repository.Add(user);
        await repository.Add(user2);

        //Act
        var result = await repository.GetAll();

        //Assert
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAll_ShouldGetAllUsersWhere()
    {
        //Arrange
        var options = GetInMemoryOptions();
        var contextFactory = new DbContextFactory<ApplicationDbContext>(options);
        var repository = new Repository<User>(contextFactory);

        var user = new User { Name = "Test User", AuthId = "auth0|123" };
        var user2 = new User { Name = "Test User", AuthId = "auth0|1234" };
        await repository.Add(user);
        await repository.Add(user2);

        //Act
        var result = await repository.GetAll(x => x.AuthId == "auth0|123");

        //Assert
        Assert.NotEmpty(result);
        Assert.Equal(1, result.Count);
        Assert.Equal(user.AuthId, result[0].AuthId);
    }

    [Fact]
    public async Task GetAll_ShouldGetAllUsersWhere2()
    {
        //Arrange
        var options = GetInMemoryOptions();
        var contextFactory = new DbContextFactory<ApplicationDbContext>(options);
        var repository = new Repository<User>(contextFactory);

        var user = new User { Name = "Test User", AuthId = "auth0|123" };
        var user2 = new User { Name = "Test User", AuthId = "auth0|1234" };
        await repository.Add(user);
        await repository.Add(user2);

        //Act
        var result = await repository.GetAll(x => x.Name == "Test User");

        //Assert
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
        Assert.True(result.Any(x => x.AuthId == user.AuthId) && result.Any(x => x.AuthId == user2.AuthId));
    }

    [Fact]
    public async Task GetAll_ShouldFirstOrDefault()
    {
        //Arrange
        var options = GetInMemoryOptions();
        var contextFactory = new DbContextFactory<ApplicationDbContext>(options);
        var repository = new Repository<User>(contextFactory);

        var user = new User { Name = "Test User", AuthId = "auth0|123" };
        var user2 = new User { Name = "Test User", AuthId = "auth0|1234" };
        await repository.Add(user);
        await repository.Add(user2);

        //Act
        var result = await repository.FirstOrDefault(x => x.AuthId == "auth0|123");

        //Assert
        Assert.NotNull(result);
        Assert.Equal(user.AuthId, result.AuthId);
    }

    [Fact]
    public async Task GetAll_ShouldFirstOrDefault_ReturnNull()
    {
        //Arrange
        var options = GetInMemoryOptions();
        var contextFactory = new DbContextFactory<ApplicationDbContext>(options);
        var repository = new Repository<User>(contextFactory);

        var user = new User { Name = "Test User", AuthId = "auth0|123" };
        var user2 = new User { Name = "Test User", AuthId = "auth0|1234" };
        await repository.Add(user);
        await repository.Add(user2);

        //Act
        var result = await repository.FirstOrDefault(x => x.AuthId == "asfg");

        //Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Update_ShouldUpdate()
    {
        //Arrange
        var options = GetInMemoryOptions();
        var contextFactory = new DbContextFactory<ApplicationDbContext>(options);
        var repository = new Repository<User>(contextFactory);

        var user = new User { Name = "Test User", AuthId = "auth0|123" };
        var user2 = new User { Name = "Test User", AuthId = "auth0|1234" };
        await repository.Add(user);
        await repository.Add(user2);

        user2.Name = "Test User 2";

        //Act
        var result = await repository.Update(user2);

        //Assert
        Assert.True(result.Id == user2.Id);

        result = await repository.Get(user2.Id);
        Assert.Equal("Test User 2", result.Name);
    }
}