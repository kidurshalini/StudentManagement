using Microsoft.AspNetCore.Identity;

namespace StudentManagement.Common
{
    public class SeedData
    {
        public static async Task SeedRole(IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();


            var roles = new List<IdentityRole>
            {
                new IdentityRole { Name = CustomRole.Admin, NormalizedName = CustomRole.Admin },
                new IdentityRole { Name = CustomRole.Teacher, NormalizedName = CustomRole.Teacher },
                new IdentityRole { Name = CustomRole.Student, NormalizedName = CustomRole.Student }

            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role.Name))
                {
                    await roleManager.CreateAsync(role);
                }
            }
        }
    }
}
