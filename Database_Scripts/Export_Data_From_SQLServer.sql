-- =============================================
-- Export Data from SQL Server to CSV
-- Run this on your local SQL Server to export data
-- =============================================

-- Export Users
SELECT * FROM [HR-Aviation].[dbo].[Users]
ORDER BY UserId;

-- Export Configuration Categories
SELECT * FROM [HR-Aviation].[dbo].[ConfigurationCategories]
ORDER BY CategoryId;

-- Export Configuration Values
SELECT * FROM [HR-Aviation].[dbo].[ConfigurationValues]
ORDER BY ValueId;

-- Export Permissions
SELECT * FROM [HR-Aviation].[dbo].[Permissions]
ORDER BY PermissionId;

-- Export Countries
SELECT * FROM [HR-Aviation].[dbo].[Countries]
ORDER BY CountryId;

-- Export Airports
SELECT * FROM [HR-Aviation].[dbo].[Airports]
ORDER BY AirportId;

-- Export Document Types
SELECT * FROM [HR-Aviation].[dbo].[DocumentTypes]
ORDER BY TypeId;

-- Export Controllers
SELECT * FROM [HR-Aviation].[dbo].[Controllers]
ORDER BY ControllerId;

-- Export Employees
SELECT * FROM [HR-Aviation].[dbo].[Employees]
ORDER BY EmployeeID;

-- Export Certificates
SELECT * FROM [HR-Aviation].[dbo].[Certificates]
ORDER BY CertificateId;

-- Export Licenses
SELECT * FROM [HR-Aviation].[dbo].[Licenses]
ORDER BY LicenseId;

-- Export Observations
SELECT * FROM [HR-Aviation].[dbo].[Observations]
ORDER BY ObservationId;

-- Export Projects
SELECT * FROM [HR-Aviation].[dbo].[Projects]
ORDER BY ProjectId;

-- Export Notifications
SELECT * FROM [HR-Aviation].[dbo].[Notifications]
ORDER BY NotificationId;

-- Export User Activity Logs
SELECT * FROM [HR-Aviation].[dbo].[UserActivityLogs]
ORDER BY Id;

-- Export Role Permissions
SELECT * FROM [HR-Aviation].[dbo].[RolePermissions]
ORDER BY RolePermissionId;

-- Export User Department Permissions
SELECT * FROM [HR-Aviation].[dbo].[UserDepartmentPermissions]
ORDER BY UserDepartmentPermissionId;

-- Export User Menu Permissions
SELECT * FROM [HR-Aviation].[dbo].[UserMenuPermissions]
ORDER BY UserMenuPermissionId;

-- Export User Operation Permissions
SELECT * FROM [HR-Aviation].[dbo].[UserOperationPermissions]
ORDER BY UserOperationPermissionId;

-- Export User Organizational Permissions
SELECT * FROM [HR-Aviation].[dbo].[UserOrganizationalPermissions]
ORDER BY UserOrganizationalPermissionId;

-- Export Permission Logs
SELECT * FROM [HR-Aviation].[dbo].[PermissionLogs]
ORDER BY LogId;

-- Export Configuration Log
SELECT * FROM [HR-Aviation].[dbo].[ConfigurationLog]
ORDER BY LogId;

-- Export Project Divisions
SELECT * FROM [HR-Aviation].[dbo].[ProjectDivisions]
ORDER BY Id;

-- Export Project Participants
SELECT * FROM [HR-Aviation].[dbo].[ProjectParticipants]
ORDER BY Id;

-- Export Project Phases
SELECT * FROM [HR-Aviation].[dbo].[ProjectPhases]
ORDER BY Id;

-- Export Roles
SELECT * FROM [HR-Aviation].[dbo].[Roles]
ORDER BY RoleName;