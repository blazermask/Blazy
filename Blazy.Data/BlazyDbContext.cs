using Blazy.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Blazy.Data;

/// <summary>
/// Database context for the Blazy application
/// Manages all entity relationships and database operations
/// </summary>
public class BlazyDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public BlazyDbContext(DbContextOptions<BlazyDbContext> options) : base(options)
    {
    }

    // DbSets for all entities
    public override DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Dislike> Dislikes { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<PostTag> PostTags { get; set; }
    public DbSet<UserTag> UserTags { get; set; }
    public DbSet<AdminAuditLog> AdminAuditLogs { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<RegistrationRecord> RegistrationRecords { get; set; }
    public DbSet<LoginAttempt> LoginAttempts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.UserName).HasMaxLength(50);
            entity.HasIndex(e => e.UserName).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.CustomHtml);
            entity.Property(e => e.CustomCss);
            entity.HasIndex(e => e.DeletedUsername).IsUnique().HasFilter("DeletedUsername IS NOT NULL");
        });

        // Configure Post entity
        modelBuilder.Entity<Post>(entity =>
        {
            entity.Property(e => e.Content);
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

        // Configure AdminAuditLog entity
        modelBuilder.Entity<AdminAuditLog>(entity =>
        {
            entity.HasOne(e => e.Admin)
                  .WithMany()
                  .HasForeignKey(e => e.AdminId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TargetPost)
                  .WithMany()
                  .HasForeignKey(e => e.TargetPostId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TargetUser)
                  .WithMany()
                  .HasForeignKey(e => e.TargetUserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Report entity
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasOne(e => e.Reporter)
                  .WithMany()
                  .HasForeignKey(e => e.ReporterId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReviewedByAdmin)
                  .WithMany()
                  .HasForeignKey(e => e.ReviewedByAdminId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TargetPost)
                  .WithMany()
                  .HasForeignKey(e => e.TargetPostId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TargetComment)
                  .WithMany()
                  .HasForeignKey(e => e.TargetCommentId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TargetUser)
                  .WithMany()
                  .HasForeignKey(e => e.TargetUserId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Ensure a user can't report the same content twice
            entity.HasIndex(e => new { e.ReporterId, e.ContentType, e.TargetPostId }).IsUnique().HasFilter("TargetPostId IS NOT NULL");
            entity.HasIndex(e => new { e.ReporterId, e.ContentType, e.TargetCommentId }).IsUnique().HasFilter("TargetCommentId IS NOT NULL");
            entity.HasIndex(e => new { e.ReporterId, e.ContentType, e.TargetUserId }).IsUnique().HasFilter("TargetUserId IS NOT NULL");
        });

        // Configure LoginAttempt entity
        modelBuilder.Entity<LoginAttempt>(entity =>
        {
            entity.HasIndex(e => e.IpAddress);
            entity.HasIndex(e => new { e.IpAddress, e.AttemptedAt });
        });
    }
}