-- =============================================
-- Create DataProtectionKeys Table for ASP.NET Core Data Protection
-- =============================================
-- This script creates the table needed for ASP.NET Core Data Protection
-- to persist encryption keys in PostgreSQL database
-- =============================================

-- Create DataProtectionKeys table
CREATE TABLE IF NOT EXISTS "DataProtectionKeys" (
    "Id" SERIAL PRIMARY KEY,
    "FriendlyName" TEXT,
    "Xml" TEXT
);

-- Add index for better performance
CREATE INDEX IF NOT EXISTS "IX_DataProtectionKeys_FriendlyName" 
ON "DataProtectionKeys" ("FriendlyName");

-- =============================================
-- Verification Query
-- =============================================
-- Check if table was created successfully
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable
FROM information_schema.columns 
WHERE table_name = 'DataProtectionKeys'
ORDER BY ordinal_position;

-- =============================================
-- Notes
-- =============================================
/*
IMPORTANT NOTES:
1. This table is required for ASP.NET Core Data Protection to work properly
2. It stores encryption keys used for protecting cookies and session data
3. Without this table, login sessions will not persist across application restarts
4. This is especially important for cloud deployments like Railway

SECURITY:
- The keys in this table are used to encrypt/decrypt sensitive data
- Keep this table secure and backed up
- Do not manually modify the data in this table

USAGE:
- Run this script on your PostgreSQL database (both local and Railway)
- The application will automatically use this table for key management
- No manual intervention needed after table creation
*/