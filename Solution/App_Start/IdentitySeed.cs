using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Solution.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Solution.App_Start
{
    public static class IdentitySeed
    {
        public static void SeedRolesAndAdmin()
        {
            using (var context = new ApplicationDbContext())
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                string[] roles = { "Admin", "Superutilisateur", "Utilisateur" };

                foreach (var role in roles)
                {
                    if (!roleManager.RoleExists(role))
                    {
                        roleManager.Create(new IdentityRole(role));
                    }
                }

                var emailAdmin = "admin@solution.local";
                var admin = userManager.FindByEmail(emailAdmin);

                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = "admin",
                        Email = emailAdmin,
                        Nom = "Admin",
                        Prenom = "Système",
                        EmailConfirmed = true,
                        PhoneNumber = "5145551234"
                    };

                    var result = userManager.Create(admin, "Admin123");
                    if (result.Succeeded)
                    {
                        userManager.AddToRole(admin.Id, "Admin");
                    }
                }
            }
        }
    }
}