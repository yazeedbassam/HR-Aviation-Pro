-- =============================================
-- Update Admin Password to Plain Text "123"
-- =============================================
-- This script updates the admin user password to "123" as plain text
-- Works for both SQL Server and PostgreSQL databases
-- =============================================

-- =============================================
-- FOR SQL SERVER DATABASE
-- =============================================
-- Update admin password to plain text "123" in SQL Server
UPDATE Users 
SET PasswordHash = '123' 
WHERE Username = 'admin';

-- Check if update was successful
SELECT 
    UserId,
    Username, 
    PasswordHash,
    RoleName,
    'SQL Server' as DatabaseType
FROM Users 
WHERE Username = 'admin';

-- =============================================
-- FOR POSTGRESQL DATABASE (Supabase)
-- =============================================
-- Update admin password to plain text "123" in PostgreSQL
UPDATE "Users" 
SET "PasswordHash" = '123' 
WHERE "Username" = 'admin';

-- Check if update was successful
SELECT 
    id as UserId,
    "Username", 
    "PasswordHash",
    "RoleName",
    'PostgreSQL' as DatabaseType
FROM "Users" 
WHERE "Username" = 'admin';

-- =============================================
-- ALTERNATIVE: Create Admin User if Not Exists
-- =============================================

-- FOR SQL SERVER: Create admin user if doesn't exist
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, PasswordHash, RoleName)
    VALUES ('admin', '123', 'Admin');
    PRINT 'Admin user created successfully in SQL Server';
END
ELSE
BEGIN
    PRINT 'Admin user already exists in SQL Server';
END

-- FOR POSTGRESQL: Create admin user if doesn't exist
INSERT INTO "Users" ("Username", "PasswordHash", "RoleName")
SELECT 'admin', '123', 'Admin'
WHERE NOT EXISTS (SELECT 1 FROM "Users" WHERE "Username" = 'admin');

-- =============================================
-- VERIFICATION QUERIES
-- =============================================

-- SQL Server verification
SELECT 
    'SQL Server Verification' as Info,
    UserId,
    Username,
    PasswordHash,
    RoleName,
    CASE 
        WHEN PasswordHash = '123' THEN '✅ Password is correct (123)'
        ELSE '❌ Password is NOT 123'
    END as PasswordStatus
FROM Users 
WHERE Username = 'admin';

-- PostgreSQL verification  
SELECT 
    'PostgreSQL Verification' as Info,
    id as UserId,
    "Username",
    "PasswordHash",
    "RoleName",
    CASE 
        WHEN "PasswordHash" = '123' THEN '✅ Password is correct (123)'
        ELSE '❌ Password is NOT 123'
    END as PasswordStatus
FROM "Users" 
WHERE "Username" = 'admin';

-- =============================================
-- NOTES
-- =============================================
/*
IMPORTANT NOTES:
1. This script removes all password encryption
2. Passwords are now stored as plain text
3. Admin user can login with username: admin, password: 123
4. This works for both SQL Server and PostgreSQL databases
5. Make sure to run the appropriate section for your database type

SECURITY WARNING:
- Storing passwords as plain text is NOT recommended for production
- This is only for development/testing purposes
- Consider implementing proper password hashing for production use
*/