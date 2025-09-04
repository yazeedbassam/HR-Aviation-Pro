-- =====================================================
-- Supabase Quick Setup - Create controller_users table
-- =====================================================
-- Run this script in your Supabase SQL Editor
-- =====================================================

-- Create controller_users table
CREATE TABLE IF NOT EXISTS controller_users (
    userid SERIAL PRIMARY KEY,
    username VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    fullname VARCHAR(255) NOT NULL,
    email VARCHAR(255),
    role VARCHAR(50) NOT NULL DEFAULT 'Controller',
    department VARCHAR(100),
    isactive BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP NULL
);

-- Create admin user with hashed password (password: admin123)
-- The hash below is for password "admin123"
INSERT INTO controller_users (username, password, fullname, email, role, department, isactive, created_at) 
VALUES ('admin', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'System Administrator', 'admin@aviation.com', 'Admin', 'IT', true, CURRENT_TIMESTAMP)
ON CONFLICT (username) DO UPDATE SET 
    password = EXCLUDED.password,
    fullname = EXCLUDED.fullname,
    email = EXCLUDED.email,
    role = EXCLUDED.role,
    department = EXCLUDED.department,
    isactive = EXCLUDED.isactive;

-- Verify admin user was created
SELECT userid, username, fullname, email, role, department, isactive, created_at 
FROM controller_users 
WHERE username = 'admin';

-- Show all tables
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
ORDER BY table_name;