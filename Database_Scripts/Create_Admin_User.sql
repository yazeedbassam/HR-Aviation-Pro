-- =============================================
-- Create Admin User Script for PostgreSQL
-- =============================================

-- Insert default admin user (password: admin123 - hashed with BCrypt)
INSERT INTO "Users" ("Username", "PasswordHash", "RoleName") VALUES 
('admin', '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 'Admin')
ON CONFLICT ("Username") DO UPDATE SET
    "PasswordHash" = EXCLUDED."PasswordHash",
    "RoleName" = EXCLUDED."RoleName";

-- Verify admin user was created/updated
SELECT "UserId", "Username", "RoleName", "LastPermissionUpdate" 
FROM "Users" 
WHERE "Username" = 'admin';

-- Success message
DO $$
BEGIN
    RAISE NOTICE 'Admin user created/updated successfully!';
    RAISE NOTICE 'Username: admin';
    RAISE NOTICE 'Password: admin123';
END $$;