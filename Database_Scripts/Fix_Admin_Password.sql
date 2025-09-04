-- Fix Admin Password Script
-- This script updates the admin user's password to the properly hashed version of "123"

-- First, let's see the current admin user data
SELECT userid, username, passwordhash, rolename 
FROM users 
WHERE username = 'admin';

-- Update the admin password with the properly hashed version
-- Note: You need to get the hashed password from the GetHashedPassword endpoint first
-- Visit: https://your-domain.com/Account/GetHashedPassword

-- Example update (replace 'YOUR_HASHED_PASSWORD_HERE' with the actual hash):
-- UPDATE users 
-- SET passwordhash = 'YOUR_HASHED_PASSWORD_HERE'
-- WHERE username = 'admin';

-- Verify the update
SELECT userid, username, passwordhash, rolename 
FROM users 
WHERE username = 'admin';