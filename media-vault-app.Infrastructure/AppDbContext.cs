using media_vault_app.Domain.Entities;
using media_vault_app.Domain.Enums;
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
        public DbSet<GameEntry> GameEntries { get; set; }
        public DbSet<TvSeriesEntry> TvSeriesEntries { get; set; }
        public DbSet<BookEntry> BookEntries { get; set; }
        public DbSet<MangaEntry> MangaEntries { get; set; }
        public DbSet<Season> Seasons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === MediaEntry hierarchy (TPH) ===
            modelBuilder.Entity<MediaEntry>()
                .HasDiscriminator<MediaType>("MediaType")
                .HasValue<MovieEntry>(MediaType.Movie)
                .HasValue<GameEntry>(MediaType.Game)
                .HasValue<TvSeriesEntry>(MediaType.TvSeries)
                .HasValue<BookEntry>(MediaType.Book)
                .HasValue<MangaEntry>(MediaType.Manga);

            modelBuilder.Entity<MediaEntry>()
                .Property("MediaType")
                .HasMaxLength(50);

            // Configure Rating value object for MediaEntry
            modelBuilder.Entity<MediaEntry>()
                .Property(x => x.Rating)
                .HasPrecision(3, 1)
                .HasConversion(
                    rating => rating.Value,
                    value => new Rating(value));

            // Configure relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.MediaEntries)
                .WithOne()
                .HasForeignKey(me => me.OwnerId);

            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .UseCollation("NOCASE");

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .UseCollation("NOCASE");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Version)
                .IsConcurrencyToken()
                .HasDefaultValue(1);

            modelBuilder.Entity<MediaEntry>()
                .Property(mediaEntry => mediaEntry.Version)
                .IsConcurrencyToken()
                .HasDefaultValue(1);

            // === GamePcRequirements as a complex value object (no separate table/FK) ===
            modelBuilder.Entity<GameEntry>()
                .ComplexProperty<GamePcRequirements>(g => g.PcRequirements, b => b.HasDiscriminator());

            // === TvSeriesEntry → Seasons relationship ===
            modelBuilder.Entity<TvSeriesEntry>()
                .HasMany(tv => tv.Seasons)
                .WithOne(s => s.TvSeriesEntry)
                .HasForeignKey(s => s.TvSeriesEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            // === Season configuration ===
            modelBuilder.Entity<Season>()
                .Property(s => s.Rating)
                .HasPrecision(3, 1)
                .HasConversion(
                    rating => rating.Value,
                    value => new Rating(value));

        }
    }
}
