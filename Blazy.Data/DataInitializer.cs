using Blazy.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace Blazy.Data;

/// <summary>
/// Initializes the database with seed data
/// Creates admin user and sample data for testing
/// </summary>
public static class DataInitializer
{
    /// <summary>
    /// Seeds the database with initial data
    /// </summary>
    public static async Task SeedDataAsync(BlazyDbContext context, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Create roles if they don't exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole<int>("Admin"));
        }

        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole<int>("User"));
        }

        // Create admin user if it doesn't exist
        var adminUser = await userManager.FindByNameAsync("admin");
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = "admin",
                Email = "admin@blazy.com",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                Pronouns = "they/them",
                Bio = "Welcome to Blazy! I'm the admin and I'm here to help you customize your profile and connect with others. Feel free to reach out! ♡",
                CustomFont = "Comic Sans MS, cursive",
                AccentColor = "#FF69B4",
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                await userManager.AddToRoleAsync(adminUser, "User");
            }
        }

        // Create sample tags if they don't exist
        var sampleTags = new[]
        {
            "art", "music", "writing", "photography", "gaming",
            "coding", "vintage", "anime", "fashion", "food",
            "travel", "pets", "nature", "movies", "books"
        };

        foreach (var tagName in sampleTags)
        {
            if (!context.Tags.Any(t => t.Name == tagName))
            {
                context.Tags.Add(new Tag
                {
                    Name = tagName,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // Create sample users if they don't exist
        var sampleUsers = new[]
        {
            new
            {
                Username = "retro_kat",
                Email = "kat@blazy.com",
                Password = "User123!",
                FirstName = "Katherine",
                LastName = "Smith",
                Pronouns = "she/her",
                Bio = "✧･ﾟ: *✧･ﾟ:* I love 90s aesthetics and pixel art *:･ﾟ✧*:･ﾟ✧",
                CustomFont = "Verdana, sans-serif",
                AccentColor = "#9370DB",
                Tags = new[] { "art", "vintage" }
            },
            new
            {
                Username = "punk_rock_dave",
                Email = "dave@blazy.com",
                Password = "User123!",
                FirstName = "David",
                LastName = "Johnson",
                Pronouns = "he/him",
                Bio = "Music is life 🎸 | Punk rock enthusiast | Always looking for new bands",
                CustomFont = "Impact, sans-serif",
                AccentColor = "#DC143C",
                Tags = new[] { "music", "gaming" }
            },
            new
            {
                Username = "stargazer_luna",
                Email = "luna@blazy.com",
                Password = "User123!",
                FirstName = "Luna",
                LastName = "Williams",
                Pronouns = "they/them",
                Bio = "Looking at the stars and dreaming big ✨ | Writer | Dreamer",
                CustomFont = "Georgia, serif",
                AccentColor = "#4169E1",
                Tags = new[] { "writing", "books", "nature" }
            }
        };

        foreach (var sampleUser in sampleUsers)
        {
            var user = await userManager.FindByNameAsync(sampleUser.Username);
            if (user == null)
            {
                user = new User
                {
                    UserName = sampleUser.Username,
                    Email = sampleUser.Email,
                    EmailConfirmed = true,
                    FirstName = sampleUser.FirstName,
                    LastName = sampleUser.LastName,
                    Pronouns = sampleUser.Pronouns,
                    Bio = sampleUser.Bio,
                    CustomFont = sampleUser.CustomFont,
                    AccentColor = sampleUser.AccentColor,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, sampleUser.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");

                    // Add tags to user
                    foreach (var tagName in sampleUser.Tags)
                    {
                        var tag = context.Tags.FirstOrDefault(t => t.Name == tagName);
                        if (tag != null)
                        {
                            context.UserTags.Add(new UserTag
                            {
                                UserId = user.Id,
                                TagId = tag.Id
                            });
                        }
                    }
                }
            }
        }

        await context.SaveChangesAsync();

        // Create sample posts if they don't exist
        var users = context.Users.Where(u => u.UserName != "admin").ToList();
        if (users.Count >= 2)
        {
            var samplePosts = new[]
            {
                new
                {
                    UserId = users[0].Id,
                    Title = "My first post on Blazy! ♡",
                    Content = "Hey everyone! I just joined Blazy and I'm so excited to be here! I love the nostalgic vibe of this place. Feel free to check out my profile and say hi! ✨<br><br>What are you all up to today?",
                    Tags = new[] { "vintage" }
                },
                new
                {
                    UserId = users[1].Id,
                    Title = "Top 5 Punk Albums of All Time 🎸",
                    Content = "Here's my list of the greatest punk albums ever made:<br><br>1. Nevermind - Nirvana<br>2. Dookie - Green Day<br>3. American Idiot - Green Day<br>4. London Calling - The Clash<br>5. Ramones - Ramones<br><br>What's your favorite punk album?",
                    Tags = new[] { "music" }
                },
                new
                {
                    UserId = users[2].Id,
                    Title = "Midnight Thoughts 🌙",
                    Content = "Sitting here at 2am, thinking about the universe and our place in it. Sometimes the stars make me feel so small, but also so connected to everything.<br><br>What do you think about when you can't sleep?",
                    Tags = new[] { "writing", "nature" }
                },
                new
                {
                    UserId = users[0].Id,
                    Title = "New pixel art I made! 🎨",
                    Content = "Just finished this pixel art piece! It took me about 3 hours but I'm really happy with how it turned out. Let me know what you think!<br><br>I'm planning to make more soon, so stay tuned! ✧",
                    Tags = new[] { "art" }
                }
            };

            foreach (var samplePost in samplePosts)
            {
                if (!context.Posts.Any(p => p.Title == samplePost.Title))
                {
                    var post = new Post
                    {
                        UserId = samplePost.UserId,
                        Title = samplePost.Title,
                        Content = samplePost.Content,
                        CreatedAt = DateTime.UtcNow.AddHours(-new Random().Next(1, 48)),
                        IsPublished = true
                    };

                    context.Posts.Add(post);
                    await context.SaveChangesAsync();

                    // Add tags to post
                    foreach (var tagName in samplePost.Tags)
                    {
                        var tag = context.Tags.FirstOrDefault(t => t.Name == tagName);
                        if (tag != null)
                        {
                            context.PostTags.Add(new PostTag
                            {
                                PostId = post.Id,
                                TagId = tag.Id
                            });
                        }
                    }

                    await context.SaveChangesAsync();

                    // Add some sample likes
                    var otherUsers = users.Where(u => u.Id != samplePost.UserId).ToList();
                    if (otherUsers.Any())
                    {
                        foreach (var otherUser in otherUsers.Take(2))
                        {
                            context.Likes.Add(new Like
                            {
                                PostId = post.Id,
                                UserId = otherUser.Id,
                                CreatedAt = DateTime.UtcNow.AddHours(-new Random().Next(1, 24))
                            });
                        }
                    }

                    // Add some sample comments
                    var commentUsers = users.Where(u => u.Id != samplePost.UserId).ToList();
                    if (commentUsers.Any())
                    {
                        var sampleComments = new[]
                        {
                            "This is awesome! Love it! ♡",
                            "Great post! Thanks for sharing!",
                            "Welcome to Blazy! :D",
                            "Totally agree with this!",
                            "You're so talented! ✨"
                        };

                        foreach (var commentUser in commentUsers.Take(2))
                        {
                            context.Comments.Add(new Comment
                            {
                                PostId = post.Id,
                                UserId = commentUser.Id,
                                Content = sampleComments[new Random().Next(sampleComments.Length)],
                                CreatedAt = DateTime.UtcNow.AddHours(-new Random().Next(1, 24))
                            });
                        }
                    }
                }
            }
        }

        await context.SaveChangesAsync();
    }
}
