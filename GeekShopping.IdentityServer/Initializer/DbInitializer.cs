using GeekShopping.IdentityServer.Configuration;
using GeekShopping.IdentityServer.Model;
using GeekShopping.IdentityServer.Model.Context;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GeekShopping.IdentityServer.Initializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly MySQLContext? _context;
        private readonly UserManager<ApplicationUser>? _userManager;
        private readonly RoleManager<IdentityRole>? _roleManager;

        public DbInitializer(MySQLContext? context, UserManager<ApplicationUser>? userManager, RoleManager<IdentityRole>? roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            if (_roleManager!.FindByNameAsync(IdentityConfiguration.Admin).Result != null) 
            {
                _roleManager.CreateAsync(new IdentityRole(IdentityConfiguration.Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(IdentityConfiguration.Client)).GetAwaiter().GetResult();

                ApplicationUser admin = new ApplicationUser()
                {
                    UserName = "luan-admin",
                    Email = "luan-admin@mail.com",
                    EmailConfirmed = true,
                    PhoneNumber = "+55 (47) 12345-6789",
                    FirstName = "Luan",
                    LastName = "Admin"
                };

                _userManager!.CreateAsync(admin, "admin@123").GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(admin, IdentityConfiguration.Admin).GetAwaiter().GetResult();
                var adminClaims = _userManager.AddClaimsAsync(admin, new Claim[]
                {
                    new Claim(JwtClaimTypes.Name, $"{admin.FirstName} {admin.LastName}"),
                    new Claim(JwtClaimTypes.GivenName, admin.FirstName),
                    new Claim(JwtClaimTypes.FamilyName, admin.LastName),
                    new Claim(JwtClaimTypes.Role, IdentityConfiguration.Admin)
                }).Result;

                ApplicationUser client = new ApplicationUser()
                {
                    UserName = "luan-client",
                    Email = "luan-client@mail.com",
                    EmailConfirmed = true,
                    PhoneNumber = "+55 (47) 12345-6789",
                    FirstName = "Luan",
                    LastName = "Client"
                };

                _userManager!.CreateAsync(client, "client@123").GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(client, IdentityConfiguration.Client).GetAwaiter().GetResult();
                var clientClaims = _userManager.AddClaimsAsync(client, new Claim[]
                {
                    new Claim(JwtClaimTypes.Name, $"{client.FirstName} {client.LastName}"),
                    new Claim(JwtClaimTypes.GivenName, client.FirstName),
                    new Claim(JwtClaimTypes.FamilyName, client.LastName),
                    new Claim(JwtClaimTypes.Role, IdentityConfiguration.Client)
                }).Result;
            }
        }
    }
}
