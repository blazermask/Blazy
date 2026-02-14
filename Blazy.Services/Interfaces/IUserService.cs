using Blazy.Core.DTOs;

namespace Blazy.Services.Interfaces;

/// <summary>
/// Service interface for user management operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Registers a new user
    /// </summary>
    Task<(bool Success, string Message, UserDto? User)> RegisterAsync(RegisterDto model);

    /// <summary>
    /// Logs in a user
    /// </summary>
    Task<(bool Success, string Message, UserDto? User)> LoginAsync(LoginDto model);

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    Task<UserDto?> GetUserByIdAsync(int userId, int? currentUserId = null);

    /// <summary>
    /// Gets a user by username
    /// </summary>
    Task<UserDto?> GetUserByUsernameAsync(string username, int? currentUserId = null);

    /// <summary>
    /// Updates user profile
    /// </summary>
    Task<(bool Success, string Message, UserDto? User)> UpdateProfileAsync(
        int userId,
        string? firstName,
        string? lastName,
        string? pronouns,
        string? bio,
        string? customHtml,
        string? backgroundUrl,
        string? bannerUrl,
        string? musicUrl,
        string? customFont,
        string? customCss,
        string? accentColor);

    /// <summary>
    /// Searches for users
    /// </summary>
    Task<(IEnumerable<UserDto> Users, int TotalCount)> SearchUsersAsync(
        string searchTerm,
        int pageIndex,
        int pageSize,
        int? currentUserId = null);

    /// <summary>
    /// Gets subscribed users for a user
    /// </summary>
    Task<IEnumerable<UserDto>> GetSubscribedUsersAsync(int userId, int? currentUserId = null);

    /// <summary>
    /// Subscribes to a user's blog
    /// </summary>
    Task<(bool Success, string Message)> SubscribeAsync(int subscriberId, int subscribedToId);

    /// <summary>
    /// Unsubscribes from a user's blog
    /// </summary>
    Task<(bool Success, string Message)> UnsubscribeAsync(int subscriberId, int subscribedToId);

    /// <summary>
    /// Checks if current user is admin
    /// </summary>
    Task<bool> IsAdminAsync(int userId);

    /// <summary>
    /// Updates user tags
    /// </summary>
    Task<(bool Success, string Message)> UpdateUserTagsAsync(int userId, List<string> tags);
}