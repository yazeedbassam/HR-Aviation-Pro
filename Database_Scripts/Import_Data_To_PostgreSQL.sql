-- =============================================
-- Import Data to PostgreSQL from CSV files
-- Run this after exporting data from SQL Server
-- =============================================

-- Note: You need to export data from SQL Server first using Export_Data_From_SQLServer.sql
-- Then save each result as CSV file and import using the commands below

-- =============================================
-- IMPORT COMMANDS (Run these in psql or pgAdmin)
-- =============================================

-- Import Users
-- \copy "Users" FROM 'users.csv' WITH CSV HEADER;

-- Import Configuration Categories
-- \copy "ConfigurationCategories" FROM 'configuration_categories.csv' WITH CSV HEADER;

-- Import Configuration Values
-- \copy "ConfigurationValues" FROM 'configuration_values.csv' WITH CSV HEADER;

-- Import Permissions
-- \copy "Permissions" FROM 'permissions.csv' WITH CSV HEADER;

-- Import Countries
-- \copy "Countries" FROM 'countries.csv' WITH CSV HEADER;

-- Import Airports
-- \copy "Airports" FROM 'airports.csv' WITH CSV HEADER;

-- Import Document Types
-- \copy "DocumentTypes" FROM 'document_types.csv' WITH CSV HEADER;

-- Import Controllers
-- \copy "Controllers" FROM 'controllers.csv' WITH CSV HEADER;

-- Import Employees
-- \copy "Employees" FROM 'employees.csv' WITH CSV HEADER;

-- Import Certificates
-- \copy "Certificates" FROM 'certificates.csv' WITH CSV HEADER;

-- Import Licenses
-- \copy "Licenses" FROM 'licenses.csv' WITH CSV HEADER;

-- Import Observations
-- \copy "Observations" FROM 'observations.csv' WITH CSV HEADER;

-- Import Projects
-- \copy "Projects" FROM 'projects.csv' WITH CSV HEADER;

-- Import Notifications
-- \copy "Notifications" FROM 'notifications.csv' WITH CSV HEADER;

-- Import User Activity Logs
-- \copy "UserActivityLogs" FROM 'user_activity_logs.csv' WITH CSV HEADER;

-- Import Role Permissions
-- \copy "RolePermissions" FROM 'role_permissions.csv' WITH CSV HEADER;

-- Import User Department Permissions
-- \copy "UserDepartmentPermissions" FROM 'user_department_permissions.csv' WITH CSV HEADER;

-- Import User Menu Permissions
-- \copy "UserMenuPermissions" FROM 'user_menu_permissions.csv' WITH CSV HEADER;

-- Import User Operation Permissions
-- \copy "UserOperationPermissions" FROM 'user_operation_permissions.csv' WITH CSV HEADER;

-- Import User Organizational Permissions
-- \copy "UserOrganizationalPermissions" FROM 'user_organizational_permissions.csv' WITH CSV HEADER;

-- Import Permission Logs
-- \copy "PermissionLogs" FROM 'permission_logs.csv' WITH CSV HEADER;

-- Import Configuration Log
-- \copy "ConfigurationLog" FROM 'configuration_log.csv' WITH CSV HEADER;

-- Import Project Divisions
-- \copy "ProjectDivisions" FROM 'project_divisions.csv' WITH CSV HEADER;

-- Import Project Participants
-- \copy "ProjectParticipants" FROM 'project_participants.csv' WITH CSV HEADER;

-- Import Project Phases
-- \copy "ProjectPhases" FROM 'project_phases.csv' WITH CSV HEADER;

-- Import Roles
-- \copy "Roles" FROM 'roles.csv' WITH CSV HEADER;

-- =============================================
-- VERIFICATION QUERIES
-- =============================================

-- Check record counts
SELECT 'Users' as table_name, COUNT(*) as record_count FROM "Users"
UNION ALL
SELECT 'ConfigurationCategories', COUNT(*) FROM "ConfigurationCategories"
UNION ALL
SELECT 'ConfigurationValues', COUNT(*) FROM "ConfigurationValues"
UNION ALL
SELECT 'Permissions', COUNT(*) FROM "Permissions"
UNION ALL
SELECT 'Countries', COUNT(*) FROM "Countries"
UNION ALL
SELECT 'Airports', COUNT(*) FROM "Airports"
UNION ALL
SELECT 'DocumentTypes', COUNT(*) FROM "DocumentTypes"
UNION ALL
SELECT 'Controllers', COUNT(*) FROM "Controllers"
UNION ALL
SELECT 'Employees', COUNT(*) FROM "Employees"
UNION ALL
SELECT 'Certificates', COUNT(*) FROM "Certificates"
UNION ALL
SELECT 'Licenses', COUNT(*) FROM "Licenses"
UNION ALL
SELECT 'Observations', COUNT(*) FROM "Observations"
UNION ALL
SELECT 'Projects', COUNT(*) FROM "Projects"
UNION ALL
SELECT 'Notifications', COUNT(*) FROM "Notifications"
UNION ALL
SELECT 'UserActivityLogs', COUNT(*) FROM "UserActivityLogs"
UNION ALL
SELECT 'RolePermissions', COUNT(*) FROM "RolePermissions"
UNION ALL
SELECT 'UserDepartmentPermissions', COUNT(*) FROM "UserDepartmentPermissions"
UNION ALL
SELECT 'UserMenuPermissions', COUNT(*) FROM "UserMenuPermissions"
UNION ALL
SELECT 'UserOperationPermissions', COUNT(*) FROM "UserOperationPermissions"
UNION ALL
SELECT 'UserOrganizationalPermissions', COUNT(*) FROM "UserOrganizationalPermissions"
UNION ALL
SELECT 'PermissionLogs', COUNT(*) FROM "PermissionLogs"
UNION ALL
SELECT 'ConfigurationLog', COUNT(*) FROM "ConfigurationLog"
UNION ALL
SELECT 'ProjectDivisions', COUNT(*) FROM "ProjectDivisions"
UNION ALL
SELECT 'ProjectParticipants', COUNT(*) FROM "ProjectParticipants"
UNION ALL
SELECT 'ProjectPhases', COUNT(*) FROM "ProjectPhases"
UNION ALL
SELECT 'Roles', COUNT(*) FROM "Roles"
ORDER BY table_name;

-- Success message
DO $$
BEGIN
    RAISE NOTICE 'Data import completed successfully!';
    RAISE NOTICE 'Please verify the record counts above.';
END $$;