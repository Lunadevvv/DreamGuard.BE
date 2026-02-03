using DreamGuard.BE.DAL.Constants;
using DreamGuard.BE.DAL.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DreamGuard.BE.DAL.DbContext
{
    public static class DreamGuardDbContextInitialiserExtensions
    {
        public static async Task Initialise(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var initialiser = scope.ServiceProvider.GetRequiredService<DreamGuardDbContextInitialiser>();

            await initialiser.InitialiseAsync();
            await initialiser.SeedAsync();
        }
    }
    public class DreamGuardDbContextInitialiser
    {
        private readonly DreamGuardContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public DreamGuardDbContextInitialiser(DreamGuardContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task InitialiseAsync()
        {
            try
            {
                await _context.Database.EnsureDeletedAsync();
                await _context.Database.EnsureCreatedAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while initialising the database.");
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                await TrySeedAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while seeding the database.");
            }
        }

        public async Task TrySeedAsync()
        {
            // Default roles
            var adminRole = new IdentityRole(Role.Admin);
            var userRole = new IdentityRole(Role.User);
            if (_roleManager.Roles.All(r => r.Name != adminRole.Name))
            {
                await _roleManager.CreateAsync(adminRole);
            }
            if (_roleManager.Roles.All(r => r.Name != userRole.Name))
            {
                await _roleManager.CreateAsync(userRole);
            }

            // Default users
            var administrator = new User { UserName = "Admin", Email = "admin@gmail.com", DateOfBirth = new DateOnly(2004, 01, 07), Gender = Gender.Male, EmailConfirmed = true};

            if (_userManager.Users.All(u => u.UserName != administrator.UserName))
            {
                await _userManager.CreateAsync(administrator, "Admin@123");
                if (!string.IsNullOrWhiteSpace(adminRole.Name))
                {
                    await _userManager.AddToRolesAsync(administrator, new[] { adminRole.Name });
                }
            }
        }
    }
}
