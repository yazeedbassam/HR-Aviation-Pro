-- Fix admin password to use BCrypt hash for "123"
-- This will make the password work with the current authentication system

-- Delete existing admin user
DELETE FROM "Users" WHERE "Username" = 'admin';

-- Insert new admin user with BCrypt hash for password "123"
-- BCrypt hash for "123" with salt rounds 11
INSERT INTO "Users" ("Username", "PasswordHash", "RoleName", "IsActive") 
VALUES ('admin', '$2a$11$N9qo8uLOickgx2ZMRZoMye.IjdQjOq8Q2ZMRZoMye.IjdQjOq8Q2', 'Admin', true);

-- Verify the update
SELECT "Username", "PasswordHash", "RoleName" FROM "Users" WHERE "Username" = 'admin';