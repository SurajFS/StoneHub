using Identity.Application;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Identity.Infrastructure;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager) : IIdentityService
{
    public async Task<Result<Guid>> CreateUserAsync(string email, string password, string role, CancellationToken ct = default)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            return Result.Failure<Guid>("An account with this email already exists.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            return Result.Failure<Guid>(string.Join("; ", createResult.Errors.Select(e => e.Description)));

        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));

        await userManager.AddToRoleAsync(user, role);

        return Result.Success(user.Id);
    }

    public async Task<Result<UserCredentials>> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.CheckPasswordAsync(user, password))
            return Result.Failure<UserCredentials>("Invalid email or password.");

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Buyer";

        return Result.Success(new UserCredentials(user.Id, user.Email!, role));
    }
}
