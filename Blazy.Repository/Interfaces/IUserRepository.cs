using Blazy.Core.Entities;

namespace Blazy.Repository.Interfaces;

/// <summary>
/// Repository interface for User-specific operations
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets a user by username
    /// </summary>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Gets a user by email
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Gets a user with all their related data
    /// </summary>
    Task<User?> GetUserWithDetailsAsync(int userId);

    /// <summary>
    /// Gets a user by ID including if they are banned
    /// </summary>
    Task<User?> GetByIdIncludingBannedAsync(int userId);

    /// <summary>
    /// Gets users that a specific user is subscribed to
    /// </summary>
    Task<IEnumerable<User>> GetSubscribedUsersAsync(int userId);

    /// <summary>
    /// Gets subscribers of a specific user
    /// </summary>
    Task<IEnumerable<User>> GetSubscribersAsync(int userId);

    /// <summary>
    /// Checks if a user is subscribed to another user
    /// </summary>
    Task<bool> IsSubscribedAsync(int subscriberId, int subscribedToId);

    /// <summary>
    /// Gets users with pagination for search (username only)
    /// </summary>
    Task<(IEnumerable<User> Users, int TotalCount)> SearchUsersAsync(
        string searchTerm,
        int pageIndex,
        int pageSize);

    /// <summary>
    /// Subscribe a user to another user
    /// </summary>
    Task<bool> SubscribeAsync(int subscriberId, int subscribedToId);

    /// <summary>
    /// Unsubscribe a user from another user
    /// </summary>
    Task<bool> UnsubscribeAsync(int subscriberId, int subscribedToId);
}