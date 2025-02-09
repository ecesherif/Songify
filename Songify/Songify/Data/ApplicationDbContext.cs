using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Songify.Entities;

namespace Songify.Data
{
    public class ApplicationDbContext : IdentityDbContext<SongifyUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Band> Bands { get; set; }
        public DbSet<LikedSong> LikedSongs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<LikedSong>()
            .HasKey(ls => new { ls.UserId, ls.SongId });
            modelBuilder.Entity<LikedSong>()
                .HasOne(ls => ls.SongifyUser)
                .WithMany()
                .HasForeignKey(ls => ls.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<LikedSong>()
                .HasOne(ls => ls.Song)
                .WithMany(s => s.LikedSongs)
                .HasForeignKey(ls => ls.SongId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
