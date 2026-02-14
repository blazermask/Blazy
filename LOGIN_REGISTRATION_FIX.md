# Login and Registration Issues - Complete Fix Documentation

## Problem Summary

Users were experiencing two critical issues:
1. **Login Failure:** "Invalid username or password" always shown, even with correct credentials
2. **Registration Failure:** "Username '' is invalid, can only contain letters or digits" always shown

## Root Cause Analysis

The problem was caused by a conflict between two username properties in the `User` entity:
- `Username` - A custom property defined in the User entity
- `UserName` - An inherited property from `IdentityUser<int>` (used by ASP.NET Identity for authentication)

### How the Issue Occurred:

1. When creating users in the code, only the custom `Username` property was being set
2. ASP.NET Identity uses the inherited `UserName` property for authentication
3. EF Core was trying to map both properties to the database, causing column name conflicts
4. When users tried to login, the system couldn't find them because it was looking for users by `UserName` (which was null)

## Solution Implemented

### 1. Made Username a Read-Write Alias for UserName

**File:** `Blazy.Core/Entities/User.cs`

```csharp
public class User : IdentityUser<int>
{
    [NotMapped]
    [MaxLength(50)]
    public string? Username 
    { 
        get => UserName; 
        set => UserName = value; 
    }
    
    // ... rest of properties
}
```

**Key Changes:**
- Added `[NotMapped]` attribute to prevent EF Core from creating a separate database column
- Made `Username` a wrapper property that gets and sets the inherited `UserName` property
- This ensures backward compatibility - existing code using `.Username` still works
- ASP.NET Identity now sees the username values in the `UserName` property it expects

### 2. Updated UserRepository to Use UserName

**File:** `Blazy.Repository/Repositories/UserRepository.cs`

```csharp
// Changed from u.Username to u.UserName
public async Task<Blazy.Core.Entities.User?> GetByUsernameAsync(string username)
{
    return await _dbSet
        .Include(u => u.Tags)
        .ThenInclude(ut => ut.Tag)
        .Include(u => u.Posts)
        .FirstOrDefaultAsync(u => u.UserName == username && !u.IsDeleted);
}
```

### 3. Updated DataInitializer

**File:** `Blazy.Data/DataInitializer.cs`

```csharp
// Changed from Username to UserName
adminUser = new User
{
    UserName = "admin",  // Was: Username = "admin"
    Email = "admin@blazy.com",
    // ... rest of properties
};

// Updated queries to use UserName
var user = await userManager.FindByNameAsync(sampleUser.Username);
user = new User
{
    UserName = sampleUser.Username,  // Was: Username = sampleUser.Username
    Email = sampleUser.Email,
    // ... rest
};

var users = context.Users.Where(u => u.UserName != "admin").ToList();
// Was: context.Users.Where(u => u.Username != "admin")
```

### 4. Updated BlazyDbContext

**File:** `Blazy.Data/BlazyDbContext.cs`

```csharp
modelBuilder.Entity<User>(entity =>
{
    entity.Property(e => e.UserName).HasMaxLength(50);
    entity.HasIndex(e => e.UserName).IsUnique();
    entity.HasIndex(e => e.Email).IsUnique();
    entity.Property(e => e.CustomHtml);
    entity.Property(e => e.CustomCss);
});
```

## Admin Credentials

The admin user is created in `DataInitializer.cs` with the following credentials:

- **Username:** `admin`
- **Password:** `Admin123!`
- **Email:** `admin@blazy.com`

## Testing the Fix

### 1. Rebuild the Application
```bash
cd Blazy
dotnet build
```

### 2. Delete Old Database (Important!)
```bash
cd Blazy/Blazy.Web
rm -f blazy.db  # For SQLite
# OR for SQL Server LocalDB:
# Drop database BlazyDb in SQL Server Management Studio
```

### 3. Run the Application
```bash
dotnet run
```

### 4. Test Login
1. Navigate to http://localhost:5001/Account/Login
2. Enter username: `admin`
3. Enter password: `Admin123!`
4. Click Login
5. ✅ Should successfully log in

### 5. Test Registration
1. Navigate to http://localhost:5001/Account/Register
2. Fill in the form with valid data:
   - Username: `testuser`
   - Email: `test@example.com`
   - Password: `Test123!`
   - Confirm Password: `Test123!`
   - First Name: `Test`
   - Last Name: `User`
3. Click Register
4. ✅ Should successfully register and auto-login

## Files Modified

1. **Blazy.Core/Entities/User.cs**
   - Changed Username property to be a NotMapped wrapper around UserName

2. **Blazy.Repository/Repositories/UserRepository.cs**
   - Updated GetByUsernameAsync to query by UserName instead of Username
   - Updated SearchUsersAsync to search by UserName instead of Username

3. **Blazy.Data/DataInitializer.cs**
   - Updated all user creation to set UserName instead of Username
   - Updated queries to filter by UserName instead of Username

4. **Blazy.Data/BlazyDbContext.cs**
   - Updated User entity configuration to use UserName

## Technical Details

### Why [NotMapped] is Essential

Without the `[NotMapped]` attribute, EF Core would try to:
1. Create a separate column for `Username` in the database
2. Create a column for `UserName` (inherited from IdentityUser)
3. This would cause a duplicate column name error

With `[NotMapped]`, EF Core:
1. Ignores the `Username` property when creating the database schema
2. Only creates the `UserName` column (from IdentityUser)
3. The `Username` property still works in code as a convenient alias

### Backward Compatibility

The solution maintains full backward compatibility:
- All existing code that uses `user.Username` continues to work
- The property getter/setter automatically redirects to `UserName`
- No changes needed to views, DTOs, or service layer code

### Database Schema (After Fix)

```sql
CREATE TABLE "AspNetUsers" (
    "Id" INTEGER PRIMARY KEY,
    "UserName" TEXT NULL,  -- This is the only username column
    "NormalizedUserName" TEXT NULL,
    "Email" TEXT NULL,
    "NormalizedEmail" TEXT NULL,
    "FirstName" TEXT NULL,
    "LastName" TEXT NULL,
    -- ... other columns
);
```

Note: There is NO separate `Username` column - it's just the `UserName` column from IdentityUser.

## Common Issues and Solutions

### Issue: "Invalid username or password" after applying fix

**Solution:** Make sure you deleted and recreated the database after applying the fix. The old database has the wrong schema.

### Issue: "Username '' is invalid" still appearing

**Solution:** Ensure the username field in the registration form has a value. The validation error shows an empty string, meaning the form field wasn't submitted properly.

### Issue: Build errors after applying changes

**Solution:** Run `dotnet clean` and then `dotnet restore` before rebuilding:
```bash
dotnet clean
dotnet restore
dotnet build
```

## Summary

The login and registration issues were caused by a fundamental mismatch between:
- The code setting a custom `Username` property
- ASP.NET Identity expecting the inherited `UserName` property

The fix unifies these into a single property by:
1. Making `Username` a wrapper/alias for `UserName`
2. Marking it as `[NotMapped]` so EF Core doesn't create duplicate columns
3. Updating all queries and data seeding to use `UserName`

This solution:
- ✅ Fixes both login and registration
- ✅ Maintains backward compatibility
- ✅ Works with both SQLite and SQL Server
- ✅ Follows ASP.NET Identity best practices
- ✅ Requires minimal code changes