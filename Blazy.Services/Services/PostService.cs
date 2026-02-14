using Blazy.Core.DTOs;
using Blazy.Core.Entities;
using Blazy.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blazy.Services.Services;

/// <summary>
/// Service implementation for post management operations
/// </summary>
public class PostService : Interfaces.IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly Blazy.Data.BlazyDbContext _context;

    public PostService(IPostRepository postRepository, IUserRepository userRepository, Blazy.Data.BlazyDbContext context)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _context = context;
    }

    public async Task<(bool Success, string Message, PostDto? Post)> CreatePostAsync(int userId, CreatePostDto model)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return (false, "User not found.", null);
        }

        var post = new Post
        {
            UserId = userId,
            Title = model.Title,
            Content = model.Content,
            ImageUrl = model.ImageUrl,
            MusicUrl = model.MusicUrl,
            CreatedAt = DateTime.UtcNow,
            IsPublished = true
        };

        await _postRepository.AddAsync(post);

        // Add tags to post
        if (model.Tags.Any())
        {
            foreach (var tagName in model.Tags)
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                if (tag == null)
                {
                    tag = new Tag
                    {
                        Name = tagName,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Tags.Add(tag);
                    await _context.SaveChangesAsync();
                }

                var postTag = new PostTag
                {
                    PostId = post.Id,
                    TagId = tag.Id
                };
                _context.PostTags.Add(postTag);
            }
            await _context.SaveChangesAsync();
        }

        var postDto = await MapToPostDto(post, userId);
        return (true, "Post created successfully!", postDto);
    }

    public async Task<PostDto?> GetPostByIdAsync(int postId, int? currentUserId = null)
    {
        var post = await _postRepository.GetPostWithDetailsAsync(postId);
        if (post == null)
        {
            return null;
        }

        return await MapToPostDto(post, currentUserId);
    }

    public async Task<(IEnumerable<PostDto> Posts, int TotalCount)> GetPostsByUserAsync(
        int userId,
        int pageIndex,
        int pageSize,
        int? currentUserId = null)
    {
        var (posts, totalCount) = await _postRepository.GetPostsByUserAsync(userId, pageIndex, pageSize);
        
        var postDtos = new List<PostDto>();
        foreach (var post in posts)
        {
            var postDto = await MapToPostDto(post, currentUserId);
            if (postDto != null)
            {
                postDtos.Add(postDto);
            }
        }

        return (postDtos, totalCount);
    }

    public async Task<(IEnumerable<PostDto> Posts, int TotalCount)> GetNewestPostsAsync(
        int pageIndex,
        int pageSize,
        int? currentUserId = null)
    {
        var (posts, totalCount) = await _postRepository.GetNewestPostsAsync(pageIndex, pageSize);
        
        var postDtos = new List<PostDto>();
        foreach (var post in posts)
        {
            var postDto = await MapToPostDto(post, currentUserId);
            if (postDto != null)
            {
                postDtos.Add(postDto);
            }
        }

        return (postDtos, totalCount);
    }

    public async Task<(IEnumerable<PostDto> Posts, int TotalCount)> GetMostLovedPostsAsync(
        int pageIndex,
        int pageSize,
        int? currentUserId = null)
    {
        var (posts, totalCount) = await _postRepository.GetMostLovedPostsAsync(pageIndex, pageSize);
        
        var postDtos = new List<PostDto>();
        foreach (var post in posts)
        {
            var postDto = await MapToPostDto(post, currentUserId);
            if (postDto != null)
            {
                postDtos.Add(postDto);
            }
        }

        return (postDtos, totalCount);
    }

    public async Task<(IEnumerable<PostDto> Posts, int TotalCount)> GetSubscribedPostsAsync(
        int userId,
        int pageIndex,
        int pageSize,
        int? currentUserId = null)
    {
        var (posts, totalCount) = await _postRepository.GetSubscribedPostsAsync(userId, pageIndex, pageSize);
        
        var postDtos = new List<PostDto>();
        foreach (var post in posts)
        {
            var postDto = await MapToPostDto(post, currentUserId);
            if (postDto != null)
            {
                postDtos.Add(postDto);
            }
        }

        return (postDtos, totalCount);
    }

    public async Task<(IEnumerable<PostDto> Posts, int TotalCount)> SearchPostsAsync(
        string searchTerm,
        int pageIndex,
        int pageSize,
        int? currentUserId = null)
    {
        var (posts, totalCount) = await _postRepository.SearchPostsAsync(searchTerm, pageIndex, pageSize);
        
        var postDtos = new List<PostDto>();
        foreach (var post in posts)
        {
            var postDto = await MapToPostDto(post, currentUserId);
            if (postDto != null)
            {
                postDtos.Add(postDto);
            }
        }

        return (postDtos, totalCount);
    }

    public async Task<(IEnumerable<PostDto> Posts, int TotalCount)> GetPostsByTagAsync(
        string tagName,
        int pageIndex,
        int pageSize,
        int? currentUserId = null)
    {
        var (posts, totalCount) = await _postRepository.GetPostsByTagAsync(tagName, pageIndex, pageSize);
        
        var postDtos = new List<PostDto>();
        foreach (var post in posts)
        {
            var postDto = await MapToPostDto(post, currentUserId);
            if (postDto != null)
            {
                postDtos.Add(postDto);
            }
        }

        return (postDtos, totalCount);
    }

    public async Task<(bool Success, string Message)> LikePostAsync(int postId, int userId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            return (false, "Post not found.");
        }

        // Check if already liked
        var existingLike = await _context.Likes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

        if (existingLike != null)
        {
            return (false, "You have already liked this post.");
        }

        // Remove existing dislike if any
        var existingDislike = await _context.Dislikes
            .FirstOrDefaultAsync(d => d.PostId == postId && d.UserId == userId);

        if (existingDislike != null)
        {
            _context.Dislikes.Remove(existingDislike);
        }

        // Add like
        var like = new Like
        {
            PostId = postId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Likes.Add(like);
        await _context.SaveChangesAsync();

        return (true, "Post liked!");
    }

    public async Task<(bool Success, string Message)> UnlikePostAsync(int postId, int userId)
    {
        var like = await _context.Likes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

        if (like == null)
        {
            return (false, "You haven't liked this post.");
        }

        _context.Likes.Remove(like);
        await _context.SaveChangesAsync();

        return (true, "Like removed.");
    }

    public async Task<(bool Success, string Message)> DislikePostAsync(int postId, int userId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            return (false, "Post not found.");
        }

        // Check if already disliked
        var existingDislike = await _context.Dislikes
            .FirstOrDefaultAsync(d => d.PostId == postId && d.UserId == userId);

        if (existingDislike != null)
        {
            return (false, "You have already disliked this post.");
        }

        // Remove existing like if any
        var existingLike = await _context.Likes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

        if (existingLike != null)
        {
            _context.Likes.Remove(existingLike);
        }

        // Add dislike
        var dislike = new Dislike
        {
            PostId = postId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Dislikes.Add(dislike);
        await _context.SaveChangesAsync();

        return (true, "Post disliked!");
    }

    public async Task<(bool Success, string Message)> UndislikePostAsync(int postId, int userId)
    {
        var dislike = await _context.Dislikes
            .FirstOrDefaultAsync(d => d.PostId == postId && d.UserId == userId);

        if (dislike == null)
        {
            return (false, "You haven't disliked this post.");
        }

        _context.Dislikes.Remove(dislike);
        await _context.SaveChangesAsync();

        return (true, "Dislike removed.");
    }

    public async Task<(bool Success, string Message)> DeletePostAsync(int postId, int userId, bool isAdmin)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            return (false, "Post not found.");
        }

        if (post.UserId != userId && !isAdmin)
        {
            return (false, "You don't have permission to delete this post.");
        }

        post.IsDeleted = true;
        post.UpdatedAt = DateTime.UtcNow;

        await _postRepository.UpdateAsync(post);

        return (true, "Post deleted successfully!");
    }

    private async Task<PostDto> MapToPostDto(Post post, int? currentUserId)
    {
        var isLiked = currentUserId.HasValue && post.Likes.Any(l => l.UserId == currentUserId.Value);
        var isDisliked = currentUserId.HasValue && post.Dislikes.Any(d => d.UserId == currentUserId.Value);

        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            ImageUrl = post.ImageUrl,
            MusicUrl = post.MusicUrl,
            CreatedAt = post.CreatedAt,
            UserId = post.UserId,
            Username = post.User.Username,
            LikeCount = post.Likes.Count,
            DislikeCount = post.Dislikes.Count,
            CommentCount = post.Comments.Count(c => !c.IsDeleted),
            IsLiked = isLiked,
            IsDisliked = isDisliked,
            Tags = post.Tags.Select(pt => pt.Tag.Name).ToList()
        };
    }
}