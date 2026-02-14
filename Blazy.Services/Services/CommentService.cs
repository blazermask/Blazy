using Blazy.Core.DTOs;
using Blazy.Core.Entities;
using Blazy.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blazy.Services.Services;

/// <summary>
/// Service implementation for comment management operations
/// </summary>
public class CommentService : Interfaces.ICommentService
{
    private readonly Blazy.Repository.Interfaces.IRepository<Comment> _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly Blazy.Data.BlazyDbContext _context;

    public CommentService(
        Blazy.Repository.Interfaces.IRepository<Comment> commentRepository,
        IPostRepository postRepository,
        Blazy.Data.BlazyDbContext context)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _context = context;
    }

    public async Task<(bool Success, string Message, CommentDto? Comment)> CreateCommentAsync(
        int postId,
        int userId,
        CreateCommentDto model)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            return (false, "Post not found.", null);
        }

        var comment = new Comment
        {
            PostId = postId,
            UserId = userId,
            Content = model.Content,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment);

        // Reload comment with user data
        var commentWithUser = await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Post)
            .FirstOrDefaultAsync(c => c.Id == comment.Id);

        if (commentWithUser == null)
        {
            return (false, "Error creating comment.", null);
        }

        var commentDto = MapToCommentDto(commentWithUser);
        return (true, "Comment added successfully!", commentDto);
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsByPostAsync(int postId)
    {
        var comments = await _context.Comments
            .Where(c => c.PostId == postId && !c.IsDeleted)
            .Include(c => c.User)
            .Include(c => c.Post)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(MapToCommentDto);
    }

    public async Task<(bool Success, string Message)> DeleteCommentAsync(int commentId, int userId, bool isAdmin)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
        {
            return (false, "Comment not found.");
        }

        if (comment.UserId != userId && !isAdmin)
        {
            return (false, "You don't have permission to delete this comment.");
        }

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;

        await _commentRepository.UpdateAsync(comment);

        return (true, "Comment deleted successfully!");
    }

    private CommentDto MapToCommentDto(Comment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            PostId = comment.PostId,
            UserId = comment.UserId,
            Username = comment.User.Username
        };
    }
}