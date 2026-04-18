using media_vault_app.Domain.Entities;
using media_vault_app.Domain.Value_Objects;
using Microsoft.EntityFrameworkCore;

namespace media_vault_app.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<MediaEntry> MediaEntries { get; set; }
        public DbSet<MovieEntry> MovieEntries { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(u => u.MediaEntries)
                .WithOne()
                .HasForeignKey(me => me.OwnerId);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<MediaEntry>()
                .Property(x => x.Rating)
                .HasPrecision(3, 1);

            modelBuilder.Entity<MediaEntry>()
                .Property(e => e.Rating)
                .HasConversion(
                    rating => rating.Value,
                    value => new Rating(value));

            modelBuilder.Entity<MediaEntry>()
                .ToTable(t =>
                t.HasCheckConstraint("CK_MediaEntry_Rating",
                "Rating >= 0 AND Rating <= 5 AND Rating * 2 = FLOOR(Rating * 2)"));
        }
    }
}
