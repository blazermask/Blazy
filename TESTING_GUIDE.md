# Blazy Admin Features - Quick Testing Guide

## Application Access
**URL:** https://003zh.app.super.myninja.ai

## Admin Login Credentials
- **Username:** `admin`
- **Password:** `Admin123!`

## Test User Accounts
- **retro_kat** / `User123!`
- **punk_rock_dave** / `User123!`
- **stargazer_luna** / `User123!`

## Quick Test Scenarios

### Scenario 1: Delete a User Account
1. Login as admin
2. Click "Admin Panel" in navigation
3. Click "Go to Users"
4. Find user "retro_kat"
5. Click "Delete Account"
6. Enter reason: "Test deletion"
7. Check confirmation box
8. Click "Delete Account Permanently"
9. Verify success message
10. Check that user is marked as deleted
11. Verify all their posts are gone

### Scenario 2: Assign Admin Role
1. Login as admin
2. Go to Admin Panel → Manage Users
3. Find user "punk_rock_dave"
4. Click "Make Admin"
5. Confirm the dialog
6. Verify success message
7. Go back to Admin Dashboard
8. Check "Website Administrators" section
9. Verify "punk_rock_dave" is now listed as admin

### Scenario 3: Revoke Admin Role
1. Login as admin
2. Go to Admin Panel (Dashboard)
3. In "Website Administrators" section, find "punk_rock_dave"
4. Click "Revoke Admin"
5. Confirm the dialog
6. Verify success message
7. Check that "punk_rock_dave" is removed from admin list
8. Go to Manage Users
9. Verify "punk_rock_dave" now shows "User" badge instead of "Admin"

### Scenario 4: View Audit Logs
1. Login as admin
2. Go to Admin Panel (Dashboard)
3. Scroll to "Recent Admin Activity" section
4. Verify all previous actions are logged:
   - Delete Account actions
   - Assign Admin Role actions
   - Revoke Admin Role actions
5. Check that each entry shows:
   - Date and time
   - Admin who performed action
   - Action type (with colored badge)
   - Target user
   - Reason (if applicable)

### Scenario 5: Test Protection Mechanisms
1. Login as admin
2. Go to Admin Panel → Manage Users
3. Try to delete the "admin" account
   - Should show "Protected" button (disabled)
4. Go to Admin Dashboard
5. Try to revoke admin role from "admin"
   - Should show "Cannot revoke" message
6. Try to delete your own account
   - Should be blocked with error message

### Scenario 6: Delete Posts as Admin
1. Login as admin
2. Browse to any user's profile (e.g., stargazer_luna)
3. Find one of their posts
4. Click on the post
5. Look for admin controls to delete the post
6. Delete the post with a reason
7. Verify it's marked as deleted
8. Check audit log for the deletion

## Expected Results

### After Deleting a User Account:
- User is marked as deleted
- Username is changed to "deleted_user_[id]_[guid]"
- All user's posts are deleted (cascade)
- Action is logged in audit log
- User cannot login anymore

### After Assigning Admin Role:
- User appears in "Website Administrators" section
- User has "Admin" badge in user list
- User can access Admin Panel
- Action is logged in audit log

### After Revoking Admin Role:
- User is removed from "Website Administrators" section
- User has "User" badge in user list
- User cannot access Admin Panel
- Action is logged in audit log

## Verification Checklist

- [ ] Admin dashboard is accessible from homepage navigation
- [ ] All admins are listed in the dashboard
- [ ] Can assign admin role to regular users
- [ ] Can revoke admin role from non-original admins
- [ ] Cannot revoke admin role from original admin
- [ ] Can delete user accounts (except original admin)
- [ ] Deleting user account also deletes all their posts
- [ ] All admin actions are logged in audit log
- [ ] Audit log shows correct details (admin, action, target, reason, time)
- [ ] Protection mechanisms work (cannot delete self, cannot delete original admin)
- [ ] Success/error messages display correctly
- [ ] Confirmation dialogs appear for destructive actions

## Troubleshooting

### If you can't login:
- Verify you're using correct credentials
- Check that the application is running
- Clear browser cookies and try again

### If admin panel is not visible:
- Verify you're logged in as admin
- Check that user has "Admin" role in database
- Try logging out and back in

### If actions fail:
- Check browser console for JavaScript errors
- Verify you're clicking "Confirm" on dialogs
- Check that you're not trying to perform protected actions

## Notes

- The original admin account (username: "admin") is protected and cannot be deleted or demoted
- All destructive actions require confirmation
- Audit logs are permanent and cannot be deleted
- Cascade deletion is automatic - you don't need to manually delete posts
- The application uses SQLite database stored in `Blazy.Web/blazy.db`

## Support

If you encounter any issues during testing, check:
1. Application logs in the console
2. Browser developer console for errors
3. Database file exists and is accessible
4. .NET 8.0 SDK is properly installed