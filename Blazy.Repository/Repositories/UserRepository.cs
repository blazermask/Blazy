using Blazy.Data;
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
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
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
                       (u.UserName.Contains(searchTerm) ||
                        u.Email.Contains(searchTerm) ||
                        (u.FirstName != null && u.FirstName.Contains(searchTerm)) ||
                        (u.LastName != null && u.LastName.Contains(searchTerm))));

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.Username)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }
}