using Blazy.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Blazy.Data;

/// <summary>
/// Database context for the Blazy application
/// Manages all entity relationships and database operations
/// </summary>
public class BlazyDbContext : IdentityDbContext
{
    public BlazyDbContext(DbContextOptions<BlazyDbContext> options) : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Dislike> Dislikes { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<PostTag> PostTags { get; set; }
    public DbSet<UserTag> UserTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.CustomHtml).HasColumnType("nvarchar(max)");
            entity.Property(e => e.CustomCss).HasColumnType("nvarchar(max)");
        });

        // Configure Post entity
        modelBuilder.Entity<Post>(entity =>
        {
            entity.Property(e => e.Content).HasColumnType("nvarchar(max)");
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Posts)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Comment entity
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasOne(e => e.Post)
                  .WithMany(p => p.Comments)
                  .HasForeignKey(e => e.PostId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Comments)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Subscription entity (Many-to-Many between Users)
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasOne(e => e.Subscriber)
                  .WithMany(u => u.Subscriptions)
                  .HasForeignKey(e => e.SubscriberId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SubscribedTo)
                  .WithMany(u => u.Subscribers)
                  .HasForeignKey(e => e.SubscribedToId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Ensure a user can't subscribe to themselves and can only subscribe once
            entity.HasIndex(e => new { e.SubscriberId, e.SubscribedToId }).IsUnique();
        });

        // Configure Like entity
        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasOne(e => e.Post)
                  .WithMany(p => p.Likes)
                  .HasForeignKey(e => e.PostId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Likes)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Ensure a user can only like a post once
            entity.HasIndex(e => new { e.PostId, e.UserId }).IsUnique();
        });

        // Configure Dislike entity
        modelBuilder.Entity<Dislike>(entity =>
        {
            entity.HasOne(e => e.Post)
                  .WithMany(p => p.Dislikes)
                  .HasForeignKey(e => e.PostId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Dislikes)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Ensure a user can only dislike a post once
            entity.HasIndex(e => new { e.PostId, e.UserId }).IsUnique();
        });

        // Configure Tag entity
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure PostTag entity (Many-to-Many between Post and Tag)
        modelBuilder.Entity<PostTag>(entity =>
        {
            entity.HasOne(e => e.Post)
                  .WithMany(p => p.Tags)
                  .HasForeignKey(e => e.PostId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                  .WithMany(t => t.PostTags)
                  .HasForeignKey(e => e.TagId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Ensure a tag can only be applied to a post once
            entity.HasIndex(e => new { e.PostId, e.TagId }).IsUnique();
        });

        // Configure UserTag entity (Many-to-Many between User and Tag)
        modelBuilder.Entity<UserTag>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Tags)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                  .WithMany(t => t.UserTags)
                  .HasForeignKey(e => e.TagId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Ensure a tag can only be applied to a user once
            entity.HasIndex(e => new { e.UserId, e.TagId }).IsUnique();
        });
    }
}