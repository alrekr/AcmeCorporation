using AcmeCorporation.Library.Datacontracts;
using AcmeCorporation.Library.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;

namespace AcmeCorporation.Tests;

[TestFixture]
public class AdminUserSeederTests
{
    private UserManager<AcmeCorporationUser> _mockUserManager;
    private RoleManager<IdentityRole> _mockRoleManager;
    private IOptions<AdminOptions> _mockOptions;
    private ILogger<AdminUserSeeder> _mockLogger;

    // UUT
    private AdminUserSeeder _seeder;

    [SetUp]
    public void SetUp()
    {
        IUserStore<AcmeCorporationUser>? userStore = Substitute.For<IUserEmailStore<AcmeCorporationUser>>();
        ILookupNormalizer? normalizer = Substitute.For<ILookupNormalizer>();
        normalizer.NormalizeEmail(Arg.Any<string>()).Returns(args => args[0]?.ToString()?.ToUpperInvariant());
        _mockUserManager = Substitute.For<UserManager<AcmeCorporationUser>>(userStore, null, null, null, null, normalizer, null, null, null);

        IRoleStore<IdentityRole>? roleStore = Substitute.For<IRoleStore<IdentityRole>>();
        _mockRoleManager = Substitute.For<RoleManager<IdentityRole>>(roleStore, null, null, null, null);

        AdminOptions adminOptions = new();

        _mockOptions = Options.Create(adminOptions);
        _mockLogger = new NullLogger<AdminUserSeeder>();

        _seeder = new AdminUserSeeder(
            _mockRoleManager,
            _mockUserManager,
            _mockOptions,
            _mockLogger
        );
    }

    [TearDown]
    public void TearDown()
    {
        _mockUserManager.Dispose();
        _mockRoleManager.Dispose();
    }

    [Test]
    public async Task SeedAdminUserAsync_ShouldReturnEarly_WhenEmailIsNotConfigured()
    {
        // Arrange nothing
        // Act
        await _seeder.SeedAdminUserAsync();

        // Assert
        // Verify that we never even tried to find a user
        await _mockUserManager.DidNotReceive().FindByEmailAsync(Arg.Any<string>());

        // Verify no roles were created
        await _mockRoleManager.DidNotReceive().CreateAsync(Arg.Any<IdentityRole>());
    }

    [Test]
    public async Task SeedAdminUserAsync_ShouldReturnEarly_WhenAdminUserAlreadyExists()
    {
        // Arrange
        AdminOptions options = new() { Email = "admin@test.com" };
        IOptions<AdminOptions> mockOptions = Options.Create(options);
        _mockUserManager.FindByEmailAsync(options.Email)!.Returns(Task.FromResult(new AcmeCorporationUser()));
        _seeder = new AdminUserSeeder(
            _mockRoleManager,
            _mockUserManager,
            mockOptions,
            _mockLogger
        );

        // Act
        await _seeder.SeedAdminUserAsync();

        // Assert
        // Verify that we checked for the user
        await _mockUserManager.Received(1).FindByEmailAsync(options.Email);

        // Verify no roles were created
        await _mockUserManager.DidNotReceive().CreateAsync(Arg.Any<AcmeCorporationUser>(), Arg.Any<string>());
    }

    [Test]
    public async Task SeedAdminUserAsync_ShouldThrowException_WhenPasswordIsMissing()
    {
        // Arrange
        AdminOptions options = new()
        {
            Email = "admin@test.com",
            RoleName = "Admin",
            Password = string.Empty
        };
        IOptions<AdminOptions> mockOptions = Options.Create(options);
        _mockUserManager.FindByEmailAsync(options.Email).Returns(Task.FromResult<AcmeCorporationUser?>(null));
        _seeder = new AdminUserSeeder(
            _mockRoleManager,
            _mockUserManager,
            mockOptions, // Pass in the correct options
            _mockLogger
        );

        // Act & Assert
        InvalidOperationException ex = await _seeder.SeedAdminUserAsync().ShouldThrowAsync<InvalidOperationException>();
        ex.Message.ShouldBe("DefaultAdminUser configuration is incomplete.");
        // Verify no user creation was attempted
        await _mockUserManager.DidNotReceive().CreateAsync(Arg.Any<AcmeCorporationUser>(), Arg.Any<string>());
    }

    [Test]
    public async Task SeedAdminUserAsync_ShouldThrowException_WhenRoleNameIsMissing()
    {
        // Arrange
        AdminOptions options = new()
        {
            Email = "admin@test.com",
            Password = "SecurePassword123!",
            RoleName = string.Empty
        };
        IOptions<AdminOptions> mockOptions = Options.Create(options);
        Task emptyIdentity = Task.FromResult<AcmeCorporationUser?>(null);
        _mockUserManager.FindByEmailAsync(options.Email).Returns(emptyIdentity);
        _seeder = new AdminUserSeeder(
            _mockRoleManager,
            _mockUserManager,
            mockOptions, // Pass in the correct options
            _mockLogger
        );

        // Act & Assert
        await _seeder.SeedAdminUserAsync().ShouldThrowAsync(typeof(InvalidOperationException));
    }

    [Test]
    public async Task SeedAdminUserAsync_ShouldCreateUserAndRole_WhenUserDoesNotExist()
    {
        // Arrange
        AdminOptions options = new AdminOptions
        {
            Email = "newadmin@test.com",
            Username = "newadmin@test.com",
            Password = "SecurePassword123!",
            RoleName = "Administrator"
        };
        
        _mockUserManager.FindByEmailAsync(options.Email).Returns(Task.FromResult<AcmeCorporationUser?>(null));
        _mockRoleManager.RoleExistsAsync(options.RoleName).Returns(Task.FromResult(false));
        _mockRoleManager.CreateAsync(Arg.Any<IdentityRole>()).Returns(Task.FromResult(IdentityResult.Success));
        AcmeCorporationUser? capturedUser = null;
        _mockUserManager.CreateAsync(Arg.Do<AcmeCorporationUser>(u => capturedUser = u), Arg.Any<string>()).Returns(Task.FromResult(IdentityResult.Success));
        _mockUserManager.AddToRoleAsync(Arg.Any<AcmeCorporationUser>(), Arg.Any<string>()).Returns(Task.FromResult(IdentityResult.Success));

        _seeder = new AdminUserSeeder(
            _mockRoleManager,
            _mockUserManager,
            Options.Create(options),
            _mockLogger
        );

        // Act
        await _seeder.SeedAdminUserAsync();

        // Assert
        // Verify Role was created
        await _mockRoleManager.Received(1).CreateAsync(Arg.Is<IdentityRole>(r => r.Name == options.RoleName));

        // Verify User was created with correct properties and password
        capturedUser.ShouldNotBeNull();
        capturedUser.Email.ShouldBe(options.Email);
        capturedUser.UserName.ShouldBe(options.Username);
        capturedUser.EmailConfirmed.ShouldBeTrue();

        // Verify User was assigned to Role
        await _mockUserManager.Received(1).AddToRoleAsync(Arg.Is<AcmeCorporationUser>(u => u.Email == options.Email), options.RoleName);
    }

    [Test]
    public async Task SeedAdminUserAsync_ShouldSkipRoleCreation_WhenRoleAlreadyExists()
    {
        // Arrange
        AdminOptions options = new AdminOptions
        {
            Email = "admin@test.com",
            Password = "SecurePassword123!",
            RoleName = "ExistingRole"
        };

        _mockUserManager.FindByEmailAsync(options.Email)!.Returns(Task.FromResult<AcmeCorporationUser?>(null));
        _mockRoleManager.RoleExistsAsync(options.RoleName).Returns(Task.FromResult(true));
        _mockUserManager.CreateAsync(Arg.Any<AcmeCorporationUser>(), Arg.Any<string>()).Returns(Task.FromResult(IdentityResult.Success));
        _mockUserManager.AddToRoleAsync(Arg.Any<AcmeCorporationUser>(), Arg.Any<string>()).Returns(Task.FromResult(IdentityResult.Success));
        _seeder = new AdminUserSeeder(
            _mockRoleManager,
            _mockUserManager,
            Options.Create(options),
            _mockLogger
        );

        // Act
        await _seeder.SeedAdminUserAsync();

        // Assert
        // Verify we checked if the role exists
        await _mockRoleManager.Received(1).RoleExistsAsync(options.RoleName);

        // Verify we did NOT try to create the role
        await _mockRoleManager.DidNotReceive().CreateAsync(Arg.Any<IdentityRole>());

        // Verify we still created the user and assigned the role
        await _mockUserManager.Received(1).CreateAsync(Arg.Is<AcmeCorporationUser>(u => u.Email == options.Email), options.Password);
        await _mockUserManager.Received(1).AddToRoleAsync(Arg.Is<AcmeCorporationUser>(u => u.Email == options.Email), options.RoleName);
    }
}
