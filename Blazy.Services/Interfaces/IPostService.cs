using Blazy.Core.DTOs;

namespace Blazy.Services.Interfaces;

/// <summary>
/// Service interface for post management operations
/// </summary>
public interface IPostService
{
    /// <summary>
    /// Creates a new post
    /// </summary>
    Task<(bool Success, string Message, PostDto? Post)> CreatePostAsync(int userId, CreatePostDto model);

    /// <summary>
    /// Gets a post by ID
    /// </summary>
    Task<PostDto?> GetPostByIdAsync(int postId, int? currentUserId = null);

    /// <summary>
    /// Gets posts by user
    /// </summary>
    Task<(IEnumerable<PostDto> Posts, int TotalCount)> GetPostsByUserAsync(
        int userId,
        int pageIndex,
        int pageSize,
        int? currentUserId = null);

    /// <summary>
    /// Gets newest posts
    /// </summary>
    Task<(IEnumerable<PostDto> Posts, int TotalCount)> GetNewestPostsAsync(
        int pageIndex,
        int pageSize,
        int? currentUserId = null);

    /// <summary>
    /// Gets most loved posts
    /// </summary>
    Task<(IEnumerable<PostDto> Posts, int TotalCount)> GetMostLovedPostsAsync(
        int pageIndex,
        int pageSize,
        int? currentUserId = null);

    /// <summary>
    /// Gets posts from subscribed users
    /// </summary>
    Task<(IEnumerable<PostDto> Posts, int TotalCount)> GetSubscribedPostsAsync(
        int userId,
        int pageIndex,
        int pageSize,
        int? currentUserId = null);

    /// <summary>
    /// Searches posts
    /// </summary>
    Task<(IEnumerable<PostDto> Posts, int TotalCount)> SearchPostsAsync(
        string searchTerm,
        int pageIndex,
        int pageSize,
        int? currentUserId = null);

    /// <summary>
    /// Gets posts by tag
    /// </summary>
    Task<(IEnumerable<PostDto> Posts, int TotalCount)> GetPostsByTagAsync(
        string tagName,
        int pageIndex,
        int pageSize,
        int? currentUserId = null);

    /// <summary>
    /// Likes a post
    /// </summary>
    Task<(bool Success, string Message)> LikePostAsync(int postId, int userId);

    /// <summary>
    /// Unlikes a post
    /// </summary>
    Task<(bool Success, string Message)> UnlikePostAsync(int postId, int userId);

    /// <summary>
    /// Dislikes a post
    /// </summary>
    Task<(bool Success, string Message)> DislikePostAsync(int postId, int userId);

    /// <summary>
    /// Removes dislike from a post
    /// </summary>
    Task<(bool Success, string Message)> UndislikePostAsync(int postId, int userId);

    /// <summary>
    /// Deletes a post
    /// </summary>
    Task<(bool Success, string Message)> DeletePostAsync(int postId, int userId, bool isAdmin);

    /// <summary>
    /// Gets deleted posts for a specific user
    /// </summary>
    Task<IEnumerable<PostDto>> GetDeletedPostsByUserAsync(int userId, int? currentUserId = null);

    /// <summary>
    /// Gets all posts with pagination (including deleted ones for admin)
    /// </summary>
    Task<(IEnumerable<PostDto> Posts, int TotalCount)> GetAllPostsAsync(
        int pageIndex,
        int pageSize,
        int? currentUserId = null);

    /// <summary>
    /// User deletes their own post
    /// </summary>
    Task<(bool Success, string Message)> DeletePostByUserAsync(int postId, int userId);
}