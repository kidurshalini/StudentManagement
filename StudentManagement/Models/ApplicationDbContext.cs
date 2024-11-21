
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
             .HasOne(c => c.Grades)     
             .WithMany(g => g.Class)    
             .HasForeignKey(c => c.GradeId); 

            builder.Entity<SubjectModel>()
			.HasOne(s => s.Grades)
		    .WithMany()
		    .HasForeignKey(s => s.GradeId)
		    .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ClassRegistrationModel>()
       .HasOne(cr => cr.Registration)
       .WithMany() // RegistrationModel can have many ClassRegistrations
       .HasForeignKey(cr => cr.UserID) // Matches RegistrationModel.Id type
       .OnDelete(DeleteBehavior.Restrict);

            // Configure the one-to-many relationship with GradeModel
            builder.Entity<ClassRegistrationModel>()
                .HasOne(cr => cr.Grades)
                .WithMany() // GradeModel may have many ClassRegistrations
                .HasForeignKey(cr => cr.GradeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the one-to-many relationship with ClassModel
            builder.Entity<ClassRegistrationModel>()
                .HasOne(cr => cr.Class)
                .WithMany() // ClassModel may have many ClassRegistrations
                .HasForeignKey(cr => cr.ClassId)
                 .OnDelete(DeleteBehavior.Restrict);
        }

        public DbSet<GradeModel> Grades { get; set; }

        public DbSet<ClassModel> Class { get; set; }

        public DbSet<SubjectModel> Subject { get; set; }

        public DbSet<ClassRegistrationModel> UserAcadamic { get; set; }

    }
}