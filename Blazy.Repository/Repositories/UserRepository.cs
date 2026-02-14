using Blazy.Data;
using Blazy.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blazy.Repository.Repositories;

/// <summary>
/// Repository implementation for User-specific operations
/// </summary>
public class UserRepository : Repository<Blazy.Core.Entities.User>, Interfaces.IUserRepository
{
    public UserRepository(BlazyDbContext context) : base(context)
    {
    }

    public async Task<Blazy.Core.Entities.User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .Include(u => u.Tags)
            .ThenInclude(ut => ut.Tag)
            .Include(u => u.Posts)
            .FirstOrDefaultAsync(u => u.UserName == username && !u.IsDeleted);
    }

    public async Task<Blazy.Core.Entities.User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task<Blazy.Core.Entities.User?> GetUserWithDetailsAsync(int userId)
    {
        return await _dbSet
            .Include(u => u.Posts.Where(p => !p.IsDeleted && p.IsPublished))
            .Include(u => u.Subscribers)
            .Include(u => u.Subscriptions)
            .Include(u => u.Tags)
            .ThenInclude(ut => ut.Tag)
            .Include(u => u.BannedByAdmin)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
    }

    public async Task<Blazy.Core.Entities.User?> GetByIdIncludingBannedAsync(int userId)
    {
        return await _dbSet
            .Include(u => u.BannedByAdmin)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<IEnumerable<Blazy.Core.Entities.User>> GetSubscribedUsersAsync(int userId)
    {
        return await _context.Subscriptions
            .Where(s => s.SubscriberId == userId)
            .Select(s => s.SubscribedTo)
            .Where(u => !u.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Blazy.Core.Entities.User>> GetSubscribersAsync(int userId)
    {
        return await _context.Subscriptions
            .Where(s => s.SubscribedToId == userId)
            .Select(s => s.Subscriber)
            .Where(u => !u.IsDeleted)
            .ToListAsync();
    }

    public async Task<bool> IsSubscribedAsync(int subscriberId, int subscribedToId)
    {
        return await _context.Subscriptions
            .AnyAsync(s => s.SubscriberId == subscriberId && s.SubscribedToId == subscribedToId);
    }

    public async Task<(IEnumerable<Blazy.Core.Entities.User> Users, int TotalCount)> SearchUsersAsync(
        string searchTerm,
        int pageIndex,
        int pageSize)
    {
        var query = _dbSet
            .Where(u => !u.IsDeleted &&
                       !u.IsPermanentlyBanned && // Exclude permanently banned users
                       u.UserName.Contains(searchTerm));

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.UserName)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task<bool> SubscribeAsync(int subscriberId, int subscribedToId)
    {
        if (subscriberId == subscribedToId)
        {
            return false;
        }

        // Check if already subscribed
        if (await IsSubscribedAsync(subscriberId, subscribedToId))
        {
            return false;
        }

        var subscription = new Subscription
        {
            SubscriberId = subscriberId,
            SubscribedToId = subscribedToId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnsubscribeAsync(int subscriberId, int subscribedToId)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.SubscriberId == subscriberId && s.SubscribedToId == subscribedToId);

        if (subscription == null)
        {
            return false;
        }

        _context.Subscriptions.Remove(subscription);
        await _context.SaveChangesAsync();
        return true;
    }
}