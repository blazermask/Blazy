using Blazy.Data;
using Microsoft.EntityFrameworkCore;

namespace Blazy.Repository.Repositories;

/// <summary>
/// Repository implementation for Post-specific operations
/// </summary>
public class PostRepository : Repository<Blazy.Core.Entities.Post>, Interfaces.IPostRepository
{
    public PostRepository(BlazyDbContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<Blazy.Core.Entities.Post> Posts, int TotalCount)> GetPostsByUserAsync(
        int userId,
        int pageIndex,
        int pageSize)
    {
        var query = _dbSet
            .Where(p => p.UserId == userId && !p.IsDeleted && p.IsPublished)
            .Include(p => p.User)
            .Include(p => p.Tags)
            .ThenInclude(pt => pt.Tag)
            .Include(p => p.Likes)
            .Include(p => p.Dislikes)
            .Include(p => p.Comments)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();

        var posts = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (posts, totalCount);
    }

    public async Task<(IEnumerable<Blazy.Core.Entities.Post> Posts, int TotalCount)> GetNewestPostsAsync(
        int pageIndex,
        int pageSize)
    {
        var query = _dbSet
            .Where(p => !p.IsDeleted && p.IsPublished)
            .Include(p => p.User)
            .Include(p => p.Tags)
            .ThenInclude(pt => pt.Tag)
            .Include(p => p.Likes)
            .Include(p => p.Dislikes)
            .Include(p => p.Comments)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();

        var posts = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (posts, totalCount);
    }

    public async Task<(IEnumerable<Blazy.Core.Entities.Post> Posts, int TotalCount)> GetMostLovedPostsAsync(
        int pageIndex,
        int pageSize)
    {
        var query = _dbSet
            .Where(p => !p.IsDeleted && p.IsPublished)
            .Include(p => p.User)
            .Include(p => p.Tags)
            .ThenInclude(pt => pt.Tag)
            .Include(p => p.Likes)
            .Include(p => p.Dislikes)
            .Include(p => p.Comments)
            .OrderByDescending(p => p.Likes.Count)
            .ThenByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();

        var posts = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (posts, totalCount);
    }

    public async Task<(IEnumerable<Blazy.Core.Entities.Post> Posts, int TotalCount)> GetSubscribedPostsAsync(
        int userId,
        int pageIndex,
        int pageSize)
    {
        var subscribedUserIds = await _context.Subscriptions
            .Where(s => s.SubscriberId == userId)
            .Select(s => s.SubscribedToId)
            .ToListAsync();

        var query = _dbSet
            .Where(p => subscribedUserIds.Contains(p.UserId) && !p.IsDeleted && p.IsPublished)
            .Include(p => p.User)
            .Include(p => p.Tags)
            .ThenInclude(pt => pt.Tag)
            .Include(p => p.Likes)
            .Include(p => p.Dislikes)
            .Include(p => p.Comments)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();

        var posts = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (posts, totalCount);
    }

    public async Task<Blazy.Core.Entities.Post?> GetPostWithDetailsAsync(int postId)
    {
        return await _dbSet
            .Where(p => p.Id == postId && !p.IsDeleted)
            .Include(p => p.User)
            .Include(p => p.Tags)
            .ThenInclude(pt => pt.Tag)
            .Include(p => p.Comments.Where(c => !c.IsDeleted))
            .ThenInclude(c => c.User)
            .Include(p => p.Likes)
            .Include(p => p.Dislikes)
            .FirstOrDefaultAsync();
    }

    public async Task<(IEnumerable<Blazy.Core.Entities.Post> Posts, int TotalCount)> SearchPostsAsync(
        string searchTerm,
        int pageIndex,
        int pageSize)
    {
        var query = _dbSet
            .Where(p => !p.IsDeleted && p.IsPublished &&
                       (p.Title.Contains(searchTerm) || p.Content.Contains(searchTerm)))
            .Include(p => p.User)
            .Include(p => p.Tags)
            .ThenInclude(pt => pt.Tag)
            .Include(p => p.Likes)
            .Include(p => p.Dislikes)
            .Include(p => p.Comments)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();

        var posts = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (posts, totalCount);
    }

    public async Task<(IEnumerable<Blazy.Core.Entities.Post> Posts, int TotalCount)> GetPostsByTagAsync(
        string tagName,
        int pageIndex,
        int pageSize)
    {
        var query = _dbSet
            .Where(p => !p.IsDeleted && p.IsPublished &&
                       p.Tags.Any(pt => pt.Tag.Name == tagName))
            .Include(p => p.User)
            .Include(p => p.Tags)
            .ThenInclude(pt => pt.Tag)
            .Include(p => p.Likes)
            .Include(p => p.Dislikes)
            .Include(p => p.Comments)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();

        var posts = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (posts, totalCount);
    }

    public async Task<IEnumerable<Blazy.Core.Entities.Post>> GetDeletedPostsByUserAsync(int userId)
    {
        return await _dbSet
            .Where(p => p.UserId == userId && p.IsDeleted)
            .Include(p => p.User)
            .Include(p => p.DeletedByAdmin)
            .Include(p => p.Tags)
            .ThenInclude(pt => pt.Tag)
            .OrderBy(p => p.DeletionNumber)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Blazy.Core.Entities.Post> Posts, int TotalCount)> GetAllPostsAsync(
        int pageIndex,
        int pageSize)
    {
        var query = _dbSet
            .Include(p => p.User)
            .Include(p => p.Tags)
            .ThenInclude(pt => pt.Tag)
            .Include(p => p.Likes)
            .Include(p => p.Dislikes)
            .Include(p => p.Comments)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();

        var posts = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (posts, totalCount);
    }

    public async Task DeletePostsByUserAsync(int userId)
    {
        var posts = await _dbSet
            .Where(p => p.UserId == userId)
            .ToListAsync();

        _dbSet.RemoveRange(posts);
        await _context.SaveChangesAsync();
    }
}