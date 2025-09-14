using Microsoft.AspNetCore.Identity;

namespace LeaveMgmt.Infrastructure.Identity;

public static class IdentitySeeder
{
    public const string EmployeeRole = "Employee";
    public const string ManagerRole = "Manager";

    public static async Task SeedAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        try
        {
            if (!await roleManager.RoleExistsAsync(EmployeeRole))
                await roleManager.CreateAsync(new IdentityRole<Guid>(EmployeeRole));
            if (!await roleManager.RoleExistsAsync(ManagerRole))
                await roleManager.CreateAsync(new IdentityRole<Guid>(ManagerRole));
        }
        catch (Exception ex)
        {
            // log later via ILogger; for now, swallow to avoid startup crash
            Console.Error.WriteLine($"[Seed Roles] {ex}");
        }
    }
}
