
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using System.Security.Claims;
using System.Reflection.Emit;


namespace StudentManagement.Models
{
    public class ApplicationDbContext : IdentityDbContext<RegistrationModel>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ClassModel>()
             .HasOne(c => c.Grade)         // Class has one Grade
             .WithMany(g => g.Class)    // Grade has many Classes
             .HasForeignKey(c => c.GradeId); // Foreign Key in Class

            builder.Entity<SubjectModel>()
            .HasOne(s => s.Grade)        
            .WithMany(g => g.Subject)    
            .HasForeignKey(s => s.GradeId); 

        }

        public DbSet<GradeModel> Grade { get; set; }

        public DbSet<ClassModel> Class { get; set; }

        public DbSet<SubjectModel> Subject { get; set; }




    }
}