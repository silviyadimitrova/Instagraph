using Instagraph.Models;
using Microsoft.EntityFrameworkCore;

namespace Instagraph.Data
{
    public class InstagraphContext : DbContext
    {
        public InstagraphContext() { }

        public InstagraphContext(DbContextOptions options)
            :base(options) { }

		public DbSet<Picture> Pictures { get; set; }
	    public DbSet<User> Users { get; set; }
	    public DbSet<Comment> Comments { get; set; }
	    public DbSet<Post> Posts { get; set; }
	    public DbSet<UserFollower> UsersFollowers { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Configuration.ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
	        modelBuilder.Entity<User>().HasAlternateKey(e => e.Username);

	        modelBuilder.Entity<UserFollower>().HasKey(e => new {e.UserId, e.FollowerId});

	        modelBuilder.Entity<UserFollower>().HasOne(u => u.User)
		        .WithMany(u => u.Followers)
		        .HasForeignKey(uf => uf.UserId)
		        .OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<UserFollower>().HasOne(u => u.Follower)
		        .WithMany(f => f.UsersFollowing)
		        .HasForeignKey(uf => uf.FollowerId)
		        .OnDelete(DeleteBehavior.Restrict);

	        modelBuilder.Entity<User>().HasOne(u => u.ProfilePicture)
		        .WithMany(pp => pp.Users)
		        .HasForeignKey(u => u.ProfilePictureId)
		        .OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Comment>().HasOne(c => c.User)
		        .WithMany(u => u.Comments)
		        .HasForeignKey(c => c.UserId)
		        .OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Post>().HasOne(p => p.User)
		        .WithMany(u => u.Posts)
		        .HasForeignKey(u => u.UserId)
		        .OnDelete(DeleteBehavior.Restrict);

	        modelBuilder.Entity<Post>().HasOne(p => p.Picture)
		        .WithMany(pic => pic.Posts)
		        .HasForeignKey(u => u.PictureId)
		        .OnDelete(DeleteBehavior.Restrict);

	        modelBuilder.Entity<Comment>().HasOne(c => c.Post)
		        .WithMany(u => u.Comments)
		        .HasForeignKey(c => c.PostId)
		        .OnDelete(DeleteBehavior.Restrict);
		}
	}
}
