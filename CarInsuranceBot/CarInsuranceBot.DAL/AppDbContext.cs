using CarInsuranceBot.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsuranceBot.DAL
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ExtractedFields> ExtractedFields { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<Error> Errors { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<FileUploadAttempt> FileUploadAttempts { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.UserId);
                entity.Property(x => x.UserId).ValueGeneratedNever();
                entity.Property(x => x.Status).HasConversion<string>();

                entity.HasOne(x => x.ExtractedFields)
                    .WithOne(x => x.User)
                    .HasForeignKey<ExtractedFields>(x => x.UserId);

                entity.HasMany(x => x.Documents)
                    .WithOne(x => x.User)
                    .HasForeignKey(x => x.UserId);

                entity.HasOne(x => x.Policy)
                    .WithOne(x => x.User)
                    .HasForeignKey<Policy>(x => x.UserId);

                entity.HasOne(x => x.FileUploadAttempts)
                    .WithOne(x => x.User)
                    .HasForeignKey<FileUploadAttempt>(x => x.UserId);
            });

            modelBuilder.Entity<ExtractedFields>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<Policy>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Status).HasConversion<string>();
            });
            
            modelBuilder.Entity<Error>(entity =>
            {
                entity.HasKey(x => x.Id);
            });
            
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(x => x.Id);
            });
            
            modelBuilder.Entity<FileUploadAttempt>(entity =>
            {
                entity.HasKey(x => x.Id);
            });
        }
    }
}
