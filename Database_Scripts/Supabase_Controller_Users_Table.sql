-- =====================================================
-- Supabase Controller Users Table Creation
-- =====================================================
-- This script creates the controller_users table that matches the SupabaseDb.cs expectations
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

-- Create additional tables that might be needed
CREATE TABLE IF NOT EXISTS employees (
    employeeid SERIAL PRIMARY KEY,
    fullname VARCHAR(255) NOT NULL,
    email VARCHAR(255),
    phone VARCHAR(50),
    department VARCHAR(100),
    isactive BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS certificates (
    certificateid SERIAL PRIMARY KEY,
    employeeid INTEGER REFERENCES employees(employeeid),
    certificatetype VARCHAR(100),
    issuedate DATE,
    expirydate DATE,
    status VARCHAR(50),
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS observations (
    observationid SERIAL PRIMARY KEY,
    employeeid INTEGER REFERENCES employees(employeeid),
    observationtype VARCHAR(100),
    observationdate DATE,
    description TEXT,
    status VARCHAR(50),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS projects (
    projectid SERIAL PRIMARY KEY,
    projectname VARCHAR(255) NOT NULL,
    description TEXT,
    startdate DATE,
    enddate DATE,
    status VARCHAR(50),
    managerid INTEGER,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS user_activity_log (
    logid SERIAL PRIMARY KEY,
    userid INTEGER REFERENCES controller_users(userid),
    username VARCHAR(100),
    action VARCHAR(100),
    details TEXT,
    ip_address VARCHAR(45),
    user_agent TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS notifications (
    notificationid SERIAL PRIMARY KEY,
    userid INTEGER,
    controllerid INTEGER,
    message TEXT,
    link VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_read BOOLEAN DEFAULT false,
    note TEXT,
    licensetype VARCHAR(100),
    licenseexpirydate DATE
);

-- Insert sample data for testing
INSERT INTO employees (fullname, email, phone, department, isactive) VALUES
('John Doe', 'john.doe@aviation.com', '+962-79-123-4567', 'Operations', true),
('Jane Smith', 'jane.smith@aviation.com', '+962-79-234-5678', 'Maintenance', true),
('Ahmed Ali', 'ahmed.ali@aviation.com', '+962-79-345-6789', 'Security', true)
ON CONFLICT DO NOTHING;

-- Show all tables
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
ORDER BY table_name;