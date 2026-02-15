# Blazy Additional Features

## 1. Restrict Admin Password Reset (admins can only reset user passwords, not other admins)
- [x] Update `AdminService.ResetUserPasswordAsync` to block resetting any admin's password
- [x] Update the Admin Users view to hide "Reset Password" button for admin accounts
- [x] Test the restriction - backend blocks with "Cannot reset an admin's password" ✅

## 2. Login Timeout (5 minute lockout after 3 failed attempts)
- [x] Configure ASP.NET Identity lockout settings in Program.cs (3 attempts, 5 min lockout)
- [x] Update `UserService.LoginAsync` to use proper lockout-aware sign-in
- [x] Update the Login view to show lockout messages
- [x] Test lockout behavior - 3 wrong attempts → locked, even correct password blocked ✅

## 3. Account Creation Limit (2 accounts per day)
- [x] Create `RegistrationRecord` entity for IP-based tracking
- [x] Update DbContext with `RegistrationRecords` DbSet
- [x] Implement IP-based rate limiting in `UserService.RegisterAsync`
- [x] Update `AccountController.Register` to pass IP address
- [x] Test the limit - 2 accounts created, 3rd blocked ✅

## 4. Build and Test
- [x] Build with 0 errors
- [x] All 4 unit tests pass
- [x] Run and verify all features