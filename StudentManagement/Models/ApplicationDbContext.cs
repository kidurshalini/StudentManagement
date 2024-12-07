
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
                .HasForeignKey(c => c.GradeId)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.Entity<SubjectModel>()
                .HasOne(s => s.Grades)
                .WithMany()
                .HasForeignKey(s => s.GradeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ClassRegistrationModel>()
          .HasOne(cr => cr.Registration)
          .WithMany() // No reverse navigation property
          .HasForeignKey(cr => cr.UserID)
          .OnDelete(DeleteBehavior.Restrict);


            //one-to-many relationship with GradeModel
            builder.Entity<ClassRegistrationModel>()
                .HasOne(cr => cr.Grades)
                .WithMany() // 
                .HasForeignKey(cr => cr.GradeId)
                .OnDelete(DeleteBehavior.Restrict); 

           
            builder.Entity<ClassRegistrationModel>()
                .HasOne(cr => cr.Class)
                .WithMany() 
                .HasForeignKey(cr => cr.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MarksModel>()
           .HasOne(cr => cr.Registration)
           .WithMany()
           .HasForeignKey(cr => cr.UserID)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MarksModel>()
        .HasOne(s => s.Subject)
        .WithMany()
        .HasForeignKey(s => s.SubjectID)
        .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MarksModel>()
              .HasOne(c => c.Grade)
                 .WithMany()
              .HasForeignKey(c => c.GradeId)
              .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MarksModel>()
           .HasOne(c => c.Class)
           .WithMany()
           .HasForeignKey(c => c.ClassId)
           .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<contactModel>()
      .HasOne(cr => cr.Registration)
      .WithMany()
      .HasForeignKey(cr => cr.UserID)
      .OnDelete(DeleteBehavior.Restrict);
        }


        public DbSet<GradeModel> Grades { get; set; }

        public DbSet<ClassModel> Class { get; set; }

        public DbSet<SubjectModel> Subject { get; set; }

        public DbSet<ClassRegistrationModel> UserAcadamic { get; set; }
        public DbSet<MarksModel> MarksDetail { get; set; }
        public DbSet<contactModel> contactModel { get; set; }


    }
}