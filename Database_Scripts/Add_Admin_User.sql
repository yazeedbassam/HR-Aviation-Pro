-- Add Admin User to existing Users table
-- Run this in Railway MySQL Data tab if admin user doesn't exist

-- Check if admin user exists
SELECT * FROM Users WHERE Username = 'admin';

-- If no admin user exists, add one
INSERT INTO Users (Username, PasswordHash, RoleName, IsActive) 
VALUES ('admin', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Admin', TRUE)
ON DUPLICATE KEY UPDATE IsActive = TRUE;

-- Verify admin user was added
SELECT UserId, Username, RoleName, IsActive, CreatedAt FROM Users WHERE Username = 'admin'; 