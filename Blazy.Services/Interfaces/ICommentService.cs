using Blazy.Core.DTOs;

namespace Blazy.Services.Interfaces;

/// <summary>
/// Service interface for comment management operations
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// Creates a new comment
    /// </summary>
    Task<(bool Success, string Message, CommentDto? Comment)> CreateCommentAsync(
        int postId,
        int userId,
        CreateCommentDto model);

    /// <summary>
    /// Gets comments for a post
    /// </summary>
    Task<IEnumerable<CommentDto>> GetCommentsByPostAsync(int postId);

    /// <summary>
    /// Deletes a comment
    /// </summary>
    Task<(bool Success, string Message)> DeleteCommentAsync(int commentId, int userId, bool isAdmin);
}