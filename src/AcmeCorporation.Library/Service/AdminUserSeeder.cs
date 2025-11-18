using AcmeCorporation.Library.Datacontracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AcmeCorporation.Library.Service;

internal class AdminUserSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AcmeCorporationUser> _userManager;
    private readonly AdminOptions _adminOptions;
    private readonly ILogger<AdminUserSeeder> _logger;

    public AdminUserSeeder(
        RoleManager<IdentityRole> roleManager,
        UserManager<AcmeCorporationUser> userManager,
        IOptions<AdminOptions> adminOptions,
        ILogger<AdminUserSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _adminOptions = adminOptions.Value;
        _logger = logger;
    }

    public async Task SeedAdminUserAsync()
    {
        if (string.IsNullOrEmpty(_adminOptions.Email))
        {
            _logger.LogInformation("DefaultAdminUser.Email is not configured. Skipping admin user seeding.");
            return;
        }

        AcmeCorporationUser? adminUser = await _userManager.FindByEmailAsync(_adminOptions.Email);
        if (adminUser != null)
        {
            _logger.LogInformation("Default admin user '{Email}' already exists. Skipping seeding.", _adminOptions.Email);
            return;
        }
        
        if (string.IsNullOrEmpty(_adminOptions.Password) ||
            string.IsNullOrEmpty(_adminOptions.RoleName))
        {
            _logger.LogError("DefaultAdminUser.Email is configured but Password or RoleName is missing. Unable to create default admin.");
            throw new InvalidOperationException("DefaultAdminUser configuration is incomplete.");
        }

        _logger.LogInformation("Seeding default admin user '{Email}'...", _adminOptions.Email);

        if (!await _roleManager.RoleExistsAsync(_adminOptions.RoleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(_adminOptions.RoleName));
            _logger.LogInformation("Created default admin role '{RoleName}'.", _adminOptions.RoleName);
        }

        adminUser = new AcmeCorporationUser
        {
            UserName = _adminOptions.Email, // username is complicating the login.cshtml.cs, so lets use email instead.
            Email = _adminOptions.Email,
            EmailConfirmed = true,
            LockoutEnabled = false
        };

        IdentityResult result = await _userManager.CreateAsync(adminUser, _adminOptions.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(adminUser, _adminOptions.RoleName);
            _logger.LogInformation("Created default admin user '{Email}' and assigned role '{RoleName}'.", _adminOptions.Email, _adminOptions.RoleName);
        }
        else
        {
            string errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create default admin user '{Email}'. Errors: {Errors}", _adminOptions.Email, errors);
        }
    }
}