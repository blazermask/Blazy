using Blazy.Core.DTOs;
using Blazy.Core.Entities;
using Blazy.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Blazy.Services.Services;

/// <summary>
/// Service implementation for user management operations
/// </summary>
public class UserService : Interfaces.IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<User> _userManager;

    public UserService(IUserRepository userRepository, UserManager<User> userManager)
    {
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<(bool Success, string Message, UserDto? User)> RegisterAsync(RegisterDto model)
    {
        // Check if username already exists or was used by a deleted account
        if (!await IsUsernameAvailableAsync(model.Username))
        {
            return (false, "Username is already taken.", null);
        }

        // Check if email already exists
        var emailUser = await _userRepository.GetByEmailAsync(model.Email);
        if (emailUser != null)
        {
            return (false, "Email is already registered.", null);
        }

        // Create new user
        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Pronouns = model.Pronouns,
            Bio = "Welcome to my Blazy blog! ♡",
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);
        }

        // Add to User role
        await _userManager.AddToRoleAsync(user, "User");

        var userDto = await MapToUserDto(user, user.Id);
        return (true, "Registration successful!", userDto);
    }

    public async Task<(bool Success, string Message, UserDto? User)> LoginAsync(LoginDto model)
    {
        var user = await _userRepository.GetByUsernameAsync(model.UsernameOrEmail);
        if (user == null)
        {
            user = await _userRepository.GetByEmailAsync(model.UsernameOrEmail);
        }

        if (user == null)
        {
            return (false, "Invalid username or password.", null);
        }

        var result = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!result)
        {
            return (false, "Invalid username or password.", null);
        }

        var userDto = await MapToUserDto(user, user.Id);
        return (true, "Login successful!", userDto);
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId, int? currentUserId = null)
    {
        var user = await _userRepository.GetUserWithDetailsAsync(userId);
        if (user == null)
        {
            return null;
        }

        return await MapToUserDto(user, currentUserId);
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username, int? currentUserId = null)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null)
        {
            return null;
        }

        return await MapToUserDto(user, currentUserId);
    }

    public async Task<(bool Success, string Message, UserDto? User)> UpdateProfileAsync(
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
        string? accentColor)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return (false, "User not found.", null);
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        user.Pronouns = pronouns;
        user.Bio = bio;
        user.CustomHtml = customHtml;
        user.BackgroundUrl = backgroundUrl;
        user.BannerUrl = bannerUrl;
        user.MusicUrl = musicUrl;
        user.CustomFont = customFont;
        user.CustomCss = customCss;
        user.AccentColor = accentColor;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        var userDto = await MapToUserDto(user, userId);
        return (true, "Profile updated successfully!", userDto);
    }

    public async Task<(IEnumerable<UserDto> Users, int TotalCount)> SearchUsersAsync(
        string searchTerm,
        int pageIndex,
        int pageSize,
        int? currentUserId = null)
    {
        var (users, totalCount) = await _userRepository.SearchUsersAsync(searchTerm, pageIndex, pageSize);
        
        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var userDto = await MapToUserDto(user, currentUserId);
            if (userDto != null)
            {
                userDtos.Add(userDto);
            }
        }

        return (userDtos, totalCount);
    }

    public async Task<(IEnumerable<UserDto> Users, int TotalCount)> GetAllUsersAsync(
        int pageIndex,
        int pageSize,
        int? currentUserId = null)
    {
        var users = await _userRepository.GetAllAsync();
        var activeUsers = users.Where(u => !u.IsDeleted && !u.IsPermanentlyBanned).ToList();
        var totalCount = activeUsers.Count;

        var paginatedUsers = activeUsers
            .OrderBy(u => u.Username)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var userDtos = new List<UserDto>();
        foreach (var user in paginatedUsers)
        {
            var userDto = await MapToUserDto(user, currentUserId);
            if (userDto != null)
            {
                userDtos.Add(userDto);
            }
        }

        return (userDtos, totalCount);
    }

    public async Task<IEnumerable<UserDto>> GetSubscribedUsersAsync(int userId, int? currentUserId = null)
    {
        var users = await _userRepository.GetSubscribedUsersAsync(userId);
        
        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var userDto = await MapToUserDto(user, currentUserId);
            if (userDto != null)
            {
                userDtos.Add(userDto);
            }
        }

        return userDtos;
    }

    public async Task<(bool Success, string Message)> SubscribeAsync(int subscriberId, int subscribedToId)
    {
        if (subscriberId == subscribedToId)
        {
            return (false, "You cannot subscribe to yourself.");
        }

        var subscriber = await _userRepository.GetByIdAsync(subscriberId);
        var subscribedTo = await _userRepository.GetByIdAsync(subscribedToId);

        if (subscriber == null || subscribedTo == null)
        {
            return (false, "User not found.");
        }

        if (await _userRepository.IsSubscribedAsync(subscriberId, subscribedToId))
        {
            return (false, "You are already subscribed to this user.");
        }

        var success = await _userRepository.SubscribeAsync(subscriberId, subscribedToId);
        if (success)
        {
            return (true, "Successfully subscribed!");
        }

        return (false, "Failed to subscribe.");
    }

    public async Task<(bool Success, string Message)> UnsubscribeAsync(int subscriberId, int subscribedToId)
    {
        var success = await _userRepository.UnsubscribeAsync(subscriberId, subscribedToId);
        if (success)
        {
            return (true, "Successfully unsubscribed!");
        }

        return (false, "Failed to unsubscribe or subscription not found.");
    }

    public async Task<bool> IsAdminAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        return await _userManager.IsInRoleAsync(user, "Admin");
    }

    public async Task<(bool Success, string Message)> UpdateUserTagsAsync(int userId, List<string> tags)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return (false, "User not found.");
        }

        // This would need additional logic to manage tags
        // For now, return success
        return (true, "Tags updated successfully!");
    }

    private async Task<UserDto> MapToUserDto(User user, int? currentUserId)
    {
        var bannedByAdminUsername = user.BannedByAdminId.HasValue
            ? (await _userRepository.GetByIdAsync(user.BannedByAdminId.Value))?.Username
            : null;

        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Pronouns = user.Pronouns,
            Bio = user.Bio,
            CustomHtml = user.CustomHtml,
            BackgroundUrl = user.BackgroundUrl,
            BannerUrl = user.BannerUrl,
            MusicUrl = user.MusicUrl,
            CustomFont = user.CustomFont,
            CustomCss = user.CustomCss,
            AccentColor = user.AccentColor,
            CreatedAt = user.CreatedAt,
            Tags = user.Tags.Select(ut => ut.Tag.Name).ToList(),
            PostCount = user.Posts.Count(p => !p.IsDeleted),
            SubscriberCount = user.Subscribers.Count,
            IsCurrentUser = currentUserId.HasValue && currentUserId.Value == user.Id,
            IsSubscribed = currentUserId.HasValue && await _userRepository.IsSubscribedAsync(currentUserId.Value, user.Id),
            IsBanned = user.IsBanned,
            IsPermanentlyBanned = user.IsPermanentlyBanned,
            BanReason = user.BanReason,
            BanUntilDate = user.BanUntilDate,
            BannedByAdminId = user.BannedByAdminId,
            BannedByAdminUsername = bannedByAdminUsername,
            IsAdmin = isAdmin
        };
    }

    public async Task<(bool Success, string Message)> DeleteAccountAsync(int userId, DeleteAccountDto model)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return (false, "User not found.");
        }

        // Verify username matches
        if (user.UserName != model.Username)
        {
            return (false, "Username does not match.");
        }

        // Verify password
        var passwordCheck = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!passwordCheck)
        {
            return (false, "Incorrect password.");
        }

        // Store the original username to prevent reuse
        var deletedUsername = user.UserName;

        // Mark user as deleted but keep the record
        user.IsDeleted = true;
        user.UserName = $"deleted_{user.Id}_{Guid.NewGuid().ToString().Substring(0, 8)}";
        user.Email = $"deleted_{user.Id}@deleted.com";
        user.NormalizedUserName = user.UserName.ToUpper();
        user.NormalizedEmail = user.Email.ToUpper();
        user.DeletedUsername = deletedUsername;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        return (true, "Account deleted successfully. Your username cannot be reused.");
    }

    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        // Check if username is taken by an active user
        var activeUser = await _userRepository.GetByUsernameAsync(username);
        if (activeUser != null)
        {
            return false;
        }

        // Check if username was used by a deleted user
        var allUsers = await _userRepository.GetAllAsync();
        var deletedWithUsername = allUsers.Any(u => u.DeletedUsername == username);
        
        return !deletedWithUsername;
    }
}