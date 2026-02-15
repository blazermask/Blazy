using Blazy.Core.Entities;

namespace Blazy.Repository.Interfaces;

/// <summary>
/// Repository interface for Post-specific operations
/// </summary>
public interface IPostRepository : IRepository<Post>
{
    /// <summary>
    /// Gets posts for a specific user
    /// </summary>
    Task<(IEnumerable<Post> Posts, int TotalCount)> GetPostsByUserAsync(
        int userId,
        int pageIndex,
        int pageSize);

    /// <summary>
    /// Gets newest posts with pagination
    /// </summary>
    Task<(IEnumerable<Post> Posts, int TotalCount)> GetNewestPostsAsync(
        int pageIndex,
        int pageSize);

    /// <summary>
    /// Gets most loved posts (sorted by like count) with pagination
    /// </summary>
    Task<(IEnumerable<Post> Posts, int TotalCount)> GetMostLovedPostsAsync(
        int pageIndex,
        int pageSize);

    /// <summary>
    /// Gets posts from users that the current user is subscribed to
    /// </summary>
    Task<(IEnumerable<Post> Posts, int TotalCount)> GetSubscribedPostsAsync(
        int userId,
        int pageIndex,
        int pageSize);

    /// <summary>
    /// Gets a post with all related data (comments, likes, dislikes, tags)
    /// </summary>
    Task<Post?> GetPostWithDetailsAsync(int postId);

    /// <summary>
    /// Searches posts by title or content
    /// </summary>
    Task<(IEnumerable<Post> Posts, int TotalCount)> SearchPostsAsync(
        string searchTerm,
        int pageIndex,
        int pageSize);

    /// <summary>
    /// Gets posts by tag
    /// </summary>
    Task<(IEnumerable<Post> Posts, int TotalCount)> GetPostsByTagAsync(
        string tagName,
        int pageIndex,
        int pageSize);

    /// <summary>
    /// Gets deleted posts for a specific user
    /// </summary>
    Task<IEnumerable<Post>> GetDeletedPostsByUserAsync(int userId);

    /// <summary>
    /// Gets all posts with pagination (including deleted ones for admin)
    /// </summary>
    Task<(IEnumerable<Post> Posts, int TotalCount)> GetAllPostsAsync(
        int pageIndex,
        int pageSize);

    /// <summary>
    /// Deletes all posts by a specific user
    /// </summary>
    Task DeletePostsByUserAsync(int userId);

    /// <summary>
    /// Gets all comments by a specific user
    /// </summary>
    Task<IEnumerable<Blazy.Core.Entities.Comment>> GetCommentsByUserAsync(int userId);

    /// <summary>
    /// Saves all pending changes to the database
    /// </summary>
    Task SaveChangesAsync();
}