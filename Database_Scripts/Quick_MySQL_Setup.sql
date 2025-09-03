-- Quick MySQL Setup for HR Aviation System
-- Run this in Railway MySQL Data tab

-- Create Controllers table
CREATE TABLE IF NOT EXISTS Controllers (
    controllerid INT AUTO_INCREMENT PRIMARY KEY,
    fullname VARCHAR(255) NOT NULL,
    username VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    role VARCHAR(50) DEFAULT 'Controller',
    IsActive BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create admin user (password: 123)
INSERT INTO Controllers (fullname, username, password, role, IsActive) 
VALUES ('System Administrator', 'admin', '$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj4J/HS.iK2', 'Admin', TRUE)
ON DUPLICATE KEY UPDATE IsActive = TRUE;

-- Show admin user
SELECT controllerid, fullname, username, role, IsActive FROM Controllers WHERE username = 'admin'; 