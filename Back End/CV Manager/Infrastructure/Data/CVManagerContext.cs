using CV_Manager.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace CV_Manager.Infrastructure.Data
{
    /// <summary>
    /// Database context for the CV Manager application that manages the database connection
    /// and entity configurations
    /// </summary>
    public class CVManagerContext : DbContext
    {
        public CVManagerContext(DbContextOptions<CVManagerContext> options)
            : base(options)
        {
        }

        public DbSet<CV> CVs { get; set; }
        public DbSet<PersonalInformation> PersonalInformation { get; set; }
        public DbSet<ExperienceInformation> ExperienceInformation { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure CV relationship with PersonalInformation
            modelBuilder.Entity<CV>()
                .HasOne(c => c.PersonalInformation)
                .WithMany()
                .HasForeignKey(c => c.Personal_Information_Id)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure CV relationship with ExperienceInformation
            modelBuilder.Entity<CV>()
                .HasMany(c => c.Experiences)
                .WithOne(e => e.CV)
                .HasForeignKey(e => e.CVId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}