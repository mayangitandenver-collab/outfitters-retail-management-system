using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Outfitters.Domain.Entities;

namespace Outfitters.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        await db.Database.MigrateAsync();

        string[] roles =
        [
            "SuperAdministrator",
            "Administrator",
            "StoreManager",
            "Cashier",
            "InventoryClerk",
            "Auditor"
        ];

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                });
            }
        }

        var company = await db.Companies.FirstOrDefaultAsync();
        if (company is null)
        {
            company = new Company { Code = "OUT", Name = "Outfitters Apparel Store" };
            db.Companies.Add(company);
            await db.SaveChangesAsync();
        }

        var store = await db.Stores.FirstOrDefaultAsync();
        if (store is null)
        {
            store = new Store
            {
                CompanyId = company.Id,
                Code = "MAIN",
                Name = "Main Store",
                IsMainStore = true
            };
            db.Stores.Add(store);
            await db.SaveChangesAsync();
        }

        const string adminUsername = "admin";
        var admin = await userManager.FindByNameAsync(adminUsername);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminUsername,
                Email = "admin@outfitters.local",
                FirstName = "System",
                LastName = "Administrator",
                StoreId = store.Id,
                EmailConfirmed = true,
                MustChangePassword = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", result.Errors.Select(x => x.Description)));
            }

            await userManager.AddToRoleAsync(admin, "SuperAdministrator");
        }
    }
}
