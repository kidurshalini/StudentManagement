using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using System.Diagnostics;

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

        public static void SeedGrades(ApplicationDbContext context)
        {

            context.Database.EnsureCreated();


            if (!context.Grades.Any())
            {
                context.Grades.AddRange(
                    new GradeModel { Grade = 1 },
                    new GradeModel { Grade = 2 },
                    new GradeModel { Grade = 3 },
                    new GradeModel { Grade = 4 },
                    new GradeModel { Grade = 5 },
                    new GradeModel { Grade = 6 },
                    new GradeModel { Grade = 7 },
                    new GradeModel { Grade = 8 },
                    new GradeModel { Grade = 9 },
                    new GradeModel { Grade = 10 },
                    new GradeModel { Grade = 11 },
                    new GradeModel { Grade = 12 },
                    new GradeModel { Grade = 13 }
                );
                context.SaveChanges();
            }
        }
    }
}
