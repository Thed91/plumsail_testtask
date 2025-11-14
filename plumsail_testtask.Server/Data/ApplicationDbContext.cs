using Microsoft.EntityFrameworkCore;
using plumsail_testtask.Server.Models;

namespace plumsail_testtask.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<FormSubmission> FormSubmissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<FormSubmission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FormType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DataJson).IsRequired();
                entity.Property(e => e.SubmittedAt).IsRequired();
            });
        }
    }
}
