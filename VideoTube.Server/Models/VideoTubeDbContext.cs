using Microsoft.EntityFrameworkCore;

namespace VideoTube.Server.Models
{
    public class VideoTubeDbContext : DbContext
    {
        public DbSet<Like> Likes { get; set; }

        public VideoTubeDbContext(DbContextOptions<VideoTubeDbContext> options) : base(options)
        {
            // Ensure database is created
            Database.EnsureCreated();
        }
    }
} 