using GeekShopping.IdentityServer.Configuration;
using GeekShopping.IdentityServer.Model;
using GeekShopping.IdentityServer.Model.Context;
using Microsoft.AspNetCore.Identity;

namespace GeekShopping.IdentityServer.Initializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly MySQLContext? _context;
        private readonly UserManager<ApplicationUser>? _userManager;
        private readonly RoleManager<ApplicationUser>? _roleManager;

        public DbInitializer(MySQLContext? context, UserManager<ApplicationUser>? userManager, RoleManager<ApplicationUser>? roleManager)
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
            }
        }
    }
}
