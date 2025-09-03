-- =====================================================
-- AVIATION HR PRO - Supabase Database Setup
-- =====================================================
-- This script creates the database structure for Supabase (PostgreSQL)
-- =====================================================

-- =====================================================
-- STEP 1: CREATE CORE TABLES
-- =====================================================

-- Users table
CREATE TABLE IF NOT EXISTS users (
    userid SERIAL PRIMARY KEY,
    username VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'Employee',
    email VARCHAR(255),
    full_name VARCHAR(255),
    phone VARCHAR(50),
    department VARCHAR(100),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Controllers table
CREATE TABLE IF NOT EXISTS controllers (
    controller_id SERIAL PRIMARY KEY,
    userid INTEGER REFERENCES users(userid),
    username VARCHAR(100) UNIQUE NOT NULL,
    full_name VARCHAR(255) NOT NULL,
    email VARCHAR(255),
    phone VARCHAR(50),
    department VARCHAR(100),
    position VARCHAR(100),
    hire_date DATE,
    salary DECIMAL(10,2),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Employees table
CREATE TABLE IF NOT EXISTS employees (
    employee_id SERIAL PRIMARY KEY,
    userid INTEGER REFERENCES users(userid),
    username VARCHAR(100) UNIQUE NOT NULL,
    full_name VARCHAR(255) NOT NULL,
    email VARCHAR(255),
    phone VARCHAR(50),
    department VARCHAR(100),
    position VARCHAR(100),
    hire_date DATE,
    salary DECIMAL(10,2),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Countries table
CREATE TABLE IF NOT EXISTS countries (
    country_id SERIAL PRIMARY KEY,
    country_name VARCHAR(100) NOT NULL,
    country_code VARCHAR(10),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Airports table
CREATE TABLE IF NOT EXISTS airports (
    airport_id SERIAL PRIMARY KEY,
    airport_name VARCHAR(255) NOT NULL,
    airport_code VARCHAR(10),
    country_id INTEGER REFERENCES countries(country_id),
    city VARCHAR(100),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW()
);

-- =====================================================
-- STEP 2: CREATE PERMISSION TABLES
-- =====================================================

-- Permissions table
CREATE TABLE IF NOT EXISTS permissions (
    permission_id SERIAL PRIMARY KEY,
    permission_name VARCHAR(100) NOT NULL,
    permission_key VARCHAR(100) UNIQUE NOT NULL,
    permission_description TEXT,
    category_name VARCHAR(50),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW()
);

-- User Menu Permissions table
CREATE TABLE IF NOT EXISTS user_menu_permissions (
    user_menu_permission_id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(userid),
    menu_key VARCHAR(50) NOT NULL,
    is_visible BOOLEAN DEFAULT true,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- User Operation Permissions table
CREATE TABLE IF NOT EXISTS user_operation_permissions (
    user_operation_permission_id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(userid),
    permission_id INTEGER REFERENCES permissions(permission_id),
    entity_type VARCHAR(50) NOT NULL,
    operation_type VARCHAR(50) NOT NULL,
    is_allowed BOOLEAN DEFAULT true,
    scope VARCHAR(50) DEFAULT 'All',
    scope_id INTEGER,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- User Organizational Permissions table
CREATE TABLE IF NOT EXISTS user_organizational_permissions (
    user_organizational_permission_id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(userid),
    permission_type VARCHAR(50) NOT NULL,
    entity_id INTEGER NOT NULL,
    entity_name VARCHAR(100) NOT NULL,
    can_view BOOLEAN DEFAULT true,
    can_edit BOOLEAN DEFAULT false,
    can_delete BOOLEAN DEFAULT false,
    can_create BOOLEAN DEFAULT false,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- =====================================================
-- STEP 3: CREATE BUSINESS TABLES
-- =====================================================

-- Licenses table
CREATE TABLE IF NOT EXISTS licenses (
    license_id SERIAL PRIMARY KEY,
    controller_id INTEGER REFERENCES controllers(controller_id),
    license_type VARCHAR(100) NOT NULL,
    license_number VARCHAR(100) UNIQUE NOT NULL,
    issue_date DATE NOT NULL,
    expiry_date DATE NOT NULL,
    issuing_authority VARCHAR(100),
    status VARCHAR(50) DEFAULT 'Active',
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Certificates table
CREATE TABLE IF NOT EXISTS certificates (
    certificate_id SERIAL PRIMARY KEY,
    controller_id INTEGER REFERENCES controllers(controller_id),
    certificate_type VARCHAR(100) NOT NULL,
    certificate_number VARCHAR(100) UNIQUE NOT NULL,
    issue_date DATE NOT NULL,
    expiry_date DATE NOT NULL,
    issuing_authority VARCHAR(100),
    status VARCHAR(50) DEFAULT 'Active',
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Observations table
CREATE TABLE IF NOT EXISTS observations (
    observation_id SERIAL PRIMARY KEY,
    controller_id INTEGER REFERENCES controllers(controller_id),
    observation_date DATE NOT NULL,
    observation_type VARCHAR(100),
    description TEXT,
    status VARCHAR(50) DEFAULT 'Pending',
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Projects table
CREATE TABLE IF NOT EXISTS projects (
    project_id SERIAL PRIMARY KEY,
    project_name VARCHAR(255) NOT NULL,
    project_description TEXT,
    start_date DATE,
    end_date DATE,
    status VARCHAR(50) DEFAULT 'Active',
    budget DECIMAL(12,2),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- =====================================================
-- STEP 4: INSERT DEFAULT DATA
-- =====================================================

-- Insert default permissions
INSERT INTO permissions (permission_name, permission_key, permission_description, category_name) VALUES
('Add Employee', 'EMPLOYEES_ADD', 'Can add new employees', 'Staff'),
('Edit Employee', 'EMPLOYEES_EDIT', 'Can edit employee information', 'Staff'),
('Delete Employee', 'EMPLOYEES_DELETE', 'Can delete employees', 'Staff'),
('Export Employees', 'EMPLOYEES_EXPORT', 'Can export employee data', 'Staff'),
('Add Controller', 'CONTROLLERS_ADD', 'Can add new controllers', 'Staff'),
('Edit Controller', 'CONTROLLERS_EDIT', 'Can edit controller information', 'Staff'),
('Delete Controller', 'CONTROLLERS_DELETE', 'Can delete controllers', 'Staff'),
('Export Controllers', 'CONTROLLERS_EXPORT', 'Can export controller data', 'Staff'),
('Add License', 'LICENSES_ADD', 'Can add new licenses', 'Documents'),
('Edit License', 'LICENSES_EDIT', 'Can edit license information', 'Documents'),
('Delete License', 'LICENSES_DELETE', 'Can delete licenses', 'Documents'),
('Export Licenses', 'LICENSES_EXPORT', 'Can export license data', 'Documents'),
('Add Certificate', 'CERTIFICATES_ADD', 'Can add new certificates', 'Documents'),
('Edit Certificate', 'CERTIFICATES_EDIT', 'Can edit certificate information', 'Documents'),
('Delete Certificate', 'CERTIFICATES_DELETE', 'Can delete certificates', 'Documents'),
('Export Certificates', 'CERTIFICATES_EXPORT', 'Can export certificate data', 'Documents'),
('Add Observation', 'OBSERVATIONS_ADD', 'Can add new observations', 'Activities'),
('Edit Observation', 'OBSERVATIONS_EDIT', 'Can edit observation information', 'Activities'),
('Delete Observation', 'OBSERVATIONS_DELETE', 'Can delete observations', 'Activities'),
('Export Observations', 'OBSERVATIONS_EXPORT', 'Can export observation data', 'Activities'),
('View Profile Menu', 'MENU_PROFILE_VIEW', 'Can view profile in sidebar menu', 'Menu'),
('View Notifications Menu', 'MENU_NOTIFICATIONS_VIEW', 'Can view notifications in sidebar menu', 'Menu'),
('View Dashboard Menu', 'MENU_DASHBOARD_VIEW', 'Can view dashboard in sidebar menu', 'Menu'),
('View Employees Menu', 'MENU_EMPLOYEES_VIEW', 'Can view employees in sidebar menu', 'Menu'),
('View Controllers Menu', 'MENU_CONTROLLERS_VIEW', 'Can view controllers in sidebar menu', 'Menu'),
('View Licenses Menu', 'MENU_LICENSES_VIEW', 'Can view licenses in sidebar menu', 'Menu'),
('View Certificates Menu', 'MENU_CERTIFICATES_VIEW', 'Can view certificates in sidebar menu', 'Menu'),
('View Observations Menu', 'MENU_OBSERVATIONS_VIEW', 'Can view observations in sidebar menu', 'Menu'),
('View Configuration Menu', 'MENU_CONFIGURATION_VIEW', 'Can view configuration in sidebar menu', 'Menu'),
('View Permissions Menu', 'MENU_PERMISSIONS_VIEW', 'Can view permissions in sidebar menu', 'Menu'),
('Manage Permissions', 'PERMISSIONS_MANAGE', 'Can manage user permissions', 'System'),
('View Permission Logs', 'PERMISSIONS_LOGS_VIEW', 'Can view permission logs', 'System')
ON CONFLICT (permission_key) DO NOTHING;

-- Insert default countries
INSERT INTO countries (country_name, country_code) VALUES
('Saudi Arabia', 'SA'),
('United Arab Emirates', 'AE'),
('Qatar', 'QA'),
('Kuwait', 'KW'),
('Bahrain', 'BH'),
('Oman', 'OM'),
('Jordan', 'JO'),
('Egypt', 'EG'),
('Lebanon', 'LB'),
('Iraq', 'IQ')
ON CONFLICT (country_code) DO NOTHING;

-- Insert default admin user
INSERT INTO users (username, password_hash, role, full_name, email) VALUES
('admin', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Admin', 'System Administrator', 'admin@hr-aviation.com')
ON CONFLICT (username) DO NOTHING;

-- =====================================================
-- STEP 5: CREATE INDEXES FOR PERFORMANCE
-- =====================================================

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_users_username ON users(username);
CREATE INDEX IF NOT EXISTS idx_users_role ON users(role);
CREATE INDEX IF NOT EXISTS idx_controllers_userid ON controllers(userid);
CREATE INDEX IF NOT EXISTS idx_employees_userid ON employees(userid);
CREATE INDEX IF NOT EXISTS idx_licenses_controller_id ON licenses(controller_id);
CREATE INDEX IF NOT EXISTS idx_certificates_controller_id ON certificates(controller_id);
CREATE INDEX IF NOT EXISTS idx_observations_controller_id ON observations(controller_id);
CREATE INDEX IF NOT EXISTS idx_user_menu_permissions_user_id ON user_menu_permissions(user_id);
CREATE INDEX IF NOT EXISTS idx_user_operation_permissions_user_id ON user_operation_permissions(user_id);
CREATE INDEX IF NOT EXISTS idx_user_organizational_permissions_user_id ON user_organizational_permissions(user_id);

-- =====================================================
-- STEP 6: ENABLE ROW LEVEL SECURITY (RLS)
-- =====================================================

-- Enable RLS on all tables
ALTER TABLE users ENABLE ROW LEVEL SECURITY;
ALTER TABLE controllers ENABLE ROW LEVEL SECURITY;
ALTER TABLE employees ENABLE ROW LEVEL SECURITY;
ALTER TABLE licenses ENABLE ROW LEVEL SECURITY;
ALTER TABLE certificates ENABLE ROW LEVEL SECURITY;
ALTER TABLE observations ENABLE ROW LEVEL SECURITY;
ALTER TABLE projects ENABLE ROW LEVEL SECURITY;
ALTER TABLE permissions ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_menu_permissions ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_operation_permissions ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_organizational_permissions ENABLE ROW LEVEL SECURITY;

-- =====================================================
-- STEP 7: CREATE BASIC POLICIES
-- =====================================================

-- Basic policy for users table (users can view own data, admins can view all)
CREATE POLICY "Users can view own data" ON users
    FOR SELECT USING (auth.uid() = userid OR EXISTS (
        SELECT 1 FROM users WHERE userid = auth.uid() AND role = 'Admin'
    ));

-- Basic policy for controllers table
CREATE POLICY "Controllers can view own data" ON controllers
    FOR SELECT USING (userid = auth.uid() OR EXISTS (
        SELECT 1 FROM users WHERE userid = auth.uid() AND role = 'Admin'
    ));

-- =====================================================
-- COMPLETION MESSAGE
-- =====================================================

DO $$
BEGIN
    RAISE NOTICE 'Supabase database setup completed successfully!';
    RAISE NOTICE 'Default admin user created: admin / password: password';
    RAISE NOTICE 'Remember to change the default password after first login!';
END $$; 