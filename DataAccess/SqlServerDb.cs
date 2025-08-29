using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc; // Assuming this namespace remains the same
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;using System.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WebApplication1.DataAccess; // ???? ?? ??? ?????? SqlDb ???
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.DataAccess // Assuming this namespace remains the same
{
    // Renamed class from OracleDb to SqlServerDb for clarity
    public class SqlServerDb
    {
        private readonly string _connectionString;
        public string ConnectionString => _connectionString;
        private readonly ILogger<SqlServerDb> _logger; // <== ????? ??? logger
        private readonly IPasswordHasher<ControllerUser> _passwordHasher; // <== ???? ?? ??? ??? hasher ???

        // Renamed constructor from OracleDb to SqlServerDb
        public SqlServerDb(IConfiguration configuration)
        {
            // Get connection string and replace environment variables
            var connectionString = configuration.GetConnectionString("SqlServerDbConnection") 
                                ?? throw new ArgumentNullException("Connection string 'SqlServerDbConnection' not found.");
            
            // Replace environment variables in connection string
            _connectionString = ReplaceEnvironmentVariables(connectionString);
            
            // Log connection string (without sensitive data)
            var connectionStringForLogging = _connectionString;
            if (!string.IsNullOrEmpty(connectionStringForLogging))
            {
                var parts = connectionStringForLogging.Split(';');
                var safeParts = parts.Where(p => !p.ToLower().Contains("password") && !p.ToLower().Contains("pwd"));
                var safeConnectionString = string.Join(";", safeParts);
                Console.WriteLine($"Database connection configured: {safeConnectionString}");
            }
        }

        private string ReplaceEnvironmentVariables(string connectionString)
        {
            return connectionString
                .Replace("${DB_SERVER}", Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost\\SQLEXPRESS")
                .Replace("${DB_NAME}", Environment.GetEnvironmentVariable("DB_NAME") ?? "HR-Aviation")
                .Replace("${DB_USER}", Environment.GetEnvironmentVariable("DB_USER") ?? "")
                .Replace("${DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "")
                .Replace("${DB_PORT}", Environment.GetEnvironmentVariable("DB_PORT") ?? "1433");
        }

        // Constructor with dependencies
        public SqlServerDb(IConfiguration configuration, IPasswordHasher<ControllerUser> passwordHasher, ILogger<SqlServerDb> logger)
        {
            var connectionString = configuration.GetConnectionString("SqlServerDbConnection")
                                ?? throw new ArgumentNullException("Connection string 'SqlServerDbConnection' not found.");
            _connectionString = ReplaceEnvironmentVariables(connectionString);
            _passwordHasher = passwordHasher;
            _logger = logger;
        }
        // Return SqlConnection instead of OracleConnection

        public SqlConnection GetConnection()
        {
            try
            {
                var connection = new SqlConnection(_connectionString);
                Console.WriteLine("MySQL connection created successfully");
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating MySQL connection: {ex.Message}");
                throw;
            }
        }

        public bool IsDatabaseAvailable()
        {
            try
            {
                Console.WriteLine("🔍 Testing database connection...");
                Console.WriteLine($"🔍 Connection string: {_connectionString}");
                
                using var connection = GetConnection();
                Console.WriteLine("🔍 Connection created, attempting to open...");
                connection.Open();
                Console.WriteLine("✅ Database connection opened successfully");
                connection.Close();
                Console.WriteLine("✅ Database connection closed successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Database availability check failed: {ex.Message}");
                Console.WriteLine($"? Exception type: {ex.GetType().Name}");
                Console.WriteLine($"? Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        // Use SqlParameter instead of SqlParameter
        public DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            // Replace Oracle parameter placeholders (:) with MySQL placeholders (@)
            // Note: This simple replacement might fail if SQL contains colons in literals or comments.
            // A more robust regex or parser might be needed for complex cases.
            sql = sql.Replace(":", "@");

            using var conn = GetConnection(); // Uses updated GetConnection()
            conn.Open();
            using var cmd = new SqlCommand(sql, conn); // Use SqlCommand
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters); // Add SqlParameters
            }
            using var adapter = new SqlDataAdapter(cmd); // Use SqlDataAdapter
            var dt = new DataTable();
            try
            {
                adapter.Fill(dt);
            }
            catch (SqlException ex) // Catch SqlException instead of SqlException
            {
                // Log MySQL error
                Console.WriteLine($"MySQL Error: {ex.Message}");
                throw; // Re-throw the exception
            }
            // No finally needed as 'using' handles disposal/closing
            return dt;
        }

        // Use SqlParameter instead of SqlParameter
        public object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            sql = sql.Replace(":", "@"); // Replace parameter placeholders

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn); // Use SqlCommand
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters); // Add SqlParameters
            }
            return cmd.ExecuteScalar();
        }

        // Use SqlParameter instead of SqlParameter
        public int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            sql = sql.Replace(":", "@"); // Replace parameter placeholders

            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn); // Use SqlCommand
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters); // Add SqlParameters
            }
            return cmd.ExecuteNonQuery();
        }

        // --- Method Conversions ---

        //        GetAllControllers: SQL is standard, no changes needed
        public DataTable GetAllControllers()
        {
            const string sql = @"
        SELECT
          c.controllerid, c.fullname, c.username, c.airportid, c.photopath, c.licensepath,
          c.job_title, c.education_level, c.date_of_birth, c.marital_status, c.phone_number,
          c.email, c.address, c.hire_date, c.employment_status, c.current_department,
          c.transfer_date, c.emergency_contact, c.LicenseNumber, c.NeedLicense, c.IsActive,
          c.CurrentSalary, c.AnnualIncreasePercentage, c.SalaryAfterAnnualIncrease,
          c.BankAccountNumber, c.BankName, c.TaxId, c.InsuranceNumber,
          a.airportname, a.icao_code, co.CountryName
        FROM controllers c
        LEFT JOIN 
                    dbo.Airports a ON c.AirportId = a.AirportId
                LEFT JOIN 
                    dbo.Countries co ON a.CountryId = co.CountryId";
            return ExecuteQuery(sql); // Uses updated ExecuteQuery
        }

        // GetControllerById: Parameter syntax change (@ instead of :)
        public DataTable GetControllerById(int controllerId)
        {
            const string sql = @"
SELECT
  c.controllerid, c.fullname, c.username, c.airportid, c.photopath, c.licensepath,
  c.job_title, c.education_level, c.date_of_birth, c.marital_status, c.phone_number,
  c.email, c.address, c.hire_date, c.employment_status, c.current_department,
  c.transfer_date, c.emergency_contact, c.LicenseNumber, c.NeedLicense, c.IsActive,
  c.CurrentSalary, c.AnnualIncreasePercentage, c.SalaryAfterAnnualIncrease,
  c.BankAccountNumber, c.BankName, c.TaxId, c.InsuranceNumber
FROM controllers c
WHERE c.controllerid = @ctrlId"; // Changed : to @
            return ExecuteQuery(sql, new SqlParameter("@ctrlId", controllerId)); // Use SqlParameter and @
        }

        // GetObservationById: Parameter syntax change (@ instead of :)
        public Observation GetObservationById(int observationId)
        {
            const string sql = @"
        SELECT o.*, c.FullName AS ControllerName, e.FullName AS EmployeeName
        FROM dbo.Observations o
        LEFT JOIN dbo.Controllers c ON o.ControllerId = c.ControllerId
        LEFT JOIN dbo.Employees e ON o.EmployeeId = e.EmployeeID
        WHERE o.ObservationId = @ObservationId";
            var dt = ExecuteQuery(sql, new SqlParameter("@ObservationId", observationId));
            return (dt.Rows.Count > 0) ? MapDataRowToObservation(dt.Rows[0]) : null;
        }

        // UpdateController: Parameter syntax change (@ instead of :)
        public int UpdateController(Models.ControllerUser u)
        {
            const string sql = @"
UPDATE controllers SET
  fullname = @fullname, username = @username, airportid = @airportid, photopath = @photopath,
  licensepath = @licensepath, job_title = @job_title, education_level = @education_level,
  date_of_birth = @date_of_birth, marital_status = @marital_status, phone_number = @phone_number,
  email = @email, address = @address, hire_date = @hire_date, employment_status = @employment_status,
  current_department = @current_department, transfer_date = @transfer_date,
  emergency_contact = @emergency_contact, LicenseNumber = @licenseNumber,
  NeedLicense = @needLicense, IsActive = @isActive,
  CurrentSalary = @currentSalary, AnnualIncreasePercentage = @annualIncreasePercentage,
  SalaryAfterAnnualIncrease = @salaryAfterAnnualIncrease, BankAccountNumber = @bankAccountNumber,
  BankName = @bankName, TaxId = @taxId, InsuranceNumber = @insuranceNumber
WHERE controllerid = @controllerid"; // Changed : to @
            return ExecuteNonQuery(sql,
                new SqlParameter("@fullname", u.FullName),
                new SqlParameter("@username", u.Username),
                new SqlParameter("@airportid", u.AirportId),
                new SqlParameter("@photopath", (object?)u.PhotoPath ?? DBNull.Value),
                new SqlParameter("@licensepath", (object?)u.LicensePath ?? DBNull.Value),
                new SqlParameter("@job_title", (object?)u.JobTitle ?? DBNull.Value),
                new SqlParameter("@education_level", (object?)u.EducationLevel ?? DBNull.Value),
                new SqlParameter("@date_of_birth", (object?)u.DateOfBirth ?? DBNull.Value),
                new SqlParameter("@marital_status", (object?)u.MaritalStatus ?? DBNull.Value),
                new SqlParameter("@phone_number", (object?)u.PhoneNumber ?? DBNull.Value),
                new SqlParameter("@email", (object?)u.Email ?? DBNull.Value),
                new SqlParameter("@address", (object?)u.Address ?? DBNull.Value),
                new SqlParameter("@hire_date", (object?)u.HireDate ?? DBNull.Value),
                new SqlParameter("@employment_status", (object?)u.EmploymentStatus ?? DBNull.Value),
                new SqlParameter("@current_department", (object?)u.CurrentDepartment ?? DBNull.Value),
                new SqlParameter("@transfer_date", (object?)u.TransferDate ?? DBNull.Value),
                new SqlParameter("@emergency_contact", (object?)u.EmergencyContact ?? DBNull.Value),
                new SqlParameter("@licenseNumber", (object?)u.LicenseNumber ?? DBNull.Value),
                new SqlParameter("@needLicense", u.NeedLicense),
                new SqlParameter("@isActive", u.IsActive),
                new SqlParameter("@currentSalary", (object?)u.CurrentSalary ?? DBNull.Value),
                new SqlParameter("@annualIncreasePercentage", (object?)u.AnnualIncreasePercentage ?? DBNull.Value),
                new SqlParameter("@salaryAfterAnnualIncrease", (object?)u.SalaryAfterAnnualIncrease ?? DBNull.Value),
                new SqlParameter("@bankAccountNumber", (object?)u.BankAccountNumber ?? DBNull.Value),
                new SqlParameter("@bankName", (object?)u.BankName ?? DBNull.Value),
                new SqlParameter("@taxId", (object?)u.TaxId ?? DBNull.Value),
                new SqlParameter("@insuranceNumber", (object?)u.InsuranceNumber ?? DBNull.Value),
                new SqlParameter("@controllerid", u.ControllerId)
            );
        }

        // GetLicenseCountByController: Parameter syntax change (@ instead of :)
        public int GetLicenseCountByController(int controllerId)
        {
            var result = ExecuteScalar(
                "SELECT COUNT(*) FROM licenses WHERE controllerid = @controllerId", // Changed : to @
                new SqlParameter("@controllerId", controllerId) // Use SqlParameter and @
            );
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        // DeleteController: Parameter syntax change (@ instead of :), Transaction handling with SqlTransaction
        public void DeleteController(int controllerId)
        {
            using var conn = GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction(); // Uses SqlTransaction
            try
            {
                // Delete licenses first
                using var cmd1 = new SqlCommand(
                    "DELETE FROM licenses WHERE controllerid = @controllerId", conn, tx); // Use SqlCommand, @, pass tx
                cmd1.Parameters.Add(new SqlParameter("@controllerId", controllerId)); // Use SqlParameter and @
                cmd1.ExecuteNonQuery();

                // Then delete controller
                using var cmd2 = new SqlCommand(
                    "DELETE FROM controllers WHERE controllerid = @controllerId", conn, tx); // Use SqlCommand, @, pass tx
                cmd2.Parameters.Add(new SqlParameter("@controllerId", controllerId)); // Use SqlParameter and @
                cmd2.ExecuteNonQuery();

                tx.Commit();
            }
            catch (SqlException ex) // Catch SqlException
            {
                Console.WriteLine($"SQL Server Error during DeleteController: {ex.Message}");
                tx.Rollback();
                throw;
            }
        }

        // GetControllerCountByAirport: Parameter syntax change (@ instead of :)
        public int GetControllerCountByAirport(int airportId)
        {
            var result = ExecuteScalar(
                "SELECT COUNT(*) FROM controllers WHERE airportid = @id", // Changed : to @
                new SqlParameter("@id", airportId) // Use SqlParameter and @
            );
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        public List<SelectListItem> GetCountriesForSelectList()
        {
            const string sql = "SELECT CountryId, CountryName FROM Countries ORDER BY CountryName";
            var dt = ExecuteQuery(sql);
            var countries = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                countries.Add(new SelectListItem
                {
                    Value = row["CountryId"].ToString(),
                    Text = row["CountryName"].ToString()
                });
            }
            return countries;
        }
        public List<SelectListItem> GetAirportsByCountry(int countryId)
        {
            const string sql = "SELECT AirportId, AirportName FROM Airports WHERE CountryId = @CountryId ORDER BY AirportName";
            var dt = ExecuteQuery(sql, new SqlParameter("@CountryId", countryId));
            var airports = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                airports.Add(new SelectListItem
                {
                    Value = row["AirportId"].ToString(),
                    Text = row["AirportName"].ToString()
                });
            }
            return airports;
        }
        public int GetCountryIdForAirport(int airportId)
        {
            const string sql = "SELECT CountryId FROM Airports WHERE AirportId = @AirportId";
            var result = ExecuteScalar(sql, new SqlParameter("@AirportId", airportId));
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }



        // GetUserByUsername: Parameter syntax change (@ instead of :)
        public (int userId, string username, string passwordHash, string role)? GetUserByUsername(string username)
        {
            var dt = ExecuteQuery(
                "SELECT userid, username, passwordhash, rolename FROM users WHERE username = @u", // Changed : to @
                new SqlParameter("@u", username)); // Use SqlParameter and @
            if (dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];
            return (
                Convert.ToInt32(r["userid"]),
                r["username"].ToString(),
                r["passwordhash"].ToString(),
                r["rolename"].ToString()
            );
        }

        // Password Hasher remains the same (ASP.NET Core Identity)
        private readonly IPasswordHasher<ControllerUser> _hasher = new PasswordHasher<ControllerUser>();

        // CreateUser: Parameter syntax change (@ instead of :), Sequence replacement (assuming IDENTITY column for userid)
        public int CreateUser(string username, string passwordHash, string roleName)
        {
            // INSERT into the Users table (assuming this table exists and has RoleName as FK)
            // ??????? SCOPE_IDENTITY() ?????? ??? ???? ??? Identity ?????? ???? ?? ??????
            string sql = "INSERT INTO Users (Username, PasswordHash, RoleName) VALUES (@username, @passwordHash, @roleName); SELECT SCOPE_IDENTITY();";

            var parameters = new[]
            {
                new SqlParameter("@username", username),
                new SqlParameter("@passwordHash", passwordHash),
                new SqlParameter("@roleName", roleName) // <== ???? ?? ??? ?????? ?????? ?? ???? dbo.Roles
            };

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddRange(parameters);
                        object result = cmd.ExecuteScalar(); // ??????? ExecuteScalar ?????? ??? ???? ????? (??? UserId)
                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error creating user in Users table: {SQL}", sql);
                throw; // ????? ??? ????????? ?????? ???????? ?? ????? ????
            }
            return -1; // ?? ???? ????? ?? ??? ????? ????
        }

        // ValidateCredentials: Uses GetUserByUsername (already converted) and PasswordHasher (no change)
        public bool ValidateCredentials(string username, string password, out int userId, out string role)
        {
            userId = 0;
            role = null;
            var user = GetUserByUsername(username);
            if (user == null) return false;
            var (id, uname, pwHash, rl) = user.Value;
            var result = _hasher.VerifyHashedPassword(null, pwHash, password);
            if (result == PasswordVerificationResult.Success)
            {
                userId = id;
                role = rl;
                return true;
            }
            return false;
        }

        // --- Certificates --- //

        // GetAllCertificates: SQL is standard, no changes needed
        public DataTable GetAllCertificates()
        {
            const string sql = @"
SELECT cert.certificateid, cert.controllerid, ctrl.fullname AS controllername,
       cert.typeid, dt.typename AS typename, cert.certificatetitle, cert.issuingauthority, -- Use certificatetitle
       cert.issuingcountry, cert.issuedate, cert.expirydate, cert.status,
       cert.statusreason, cert.filepath, cert.notes
FROM certificates cert
JOIN controllers ctrl ON cert.controllerid = ctrl.controllerid
JOIN documenttypes dt ON cert.typeid = dt.typeid
ORDER BY cert.issuedate DESC";
            return ExecuteQuery(sql);
        }

        // GetCertificatesByController: Parameter syntax change (@ instead of :), direct execution converted
        public DataTable GetCertificatesByController(int controllerId)
        {
            const string sql = @"
SELECT certificateid, controllerid, typeid, certificatetitle, issuingauthority, issuingcountry, -- Use certificatetitle
       issuedate, expirydate, status, statusreason, filepath, notes
FROM certificates
WHERE controllerid = @controllerid -- Changed : to @
ORDER BY issuedate DESC";
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn); // Use SqlCommand
            cmd.Parameters.Add(new SqlParameter("@controllerid", controllerId)); // Use SqlParameter and @
            using var adapter = new SqlDataAdapter(cmd); // Use SqlDataAdapter
            var dt = new DataTable();
            try
            {
                adapter.Fill(dt);
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Server Error in GetCertificatesByController: {ex.Message}");
                throw;
            }
            return dt;
        }

        // DeleteCertificate: Parameter syntax change (@ instead of :)
        public int DeleteCertificate(int certificateId)
        {
            const string sql = "DELETE FROM certificates WHERE certificateid = @certId"; // Changed : to @
            return ExecuteNonQuery(sql, new SqlParameter("@certId", certificateId)); // Use SqlParameter and @
        }

        // --- Observations --- //

        // GetAllObservations: SQL is standard, no changes needed
        public DataTable GetAllObservations()
        {
            // FIX: Removed the non-existent 'travelcount' column from the SELECT statement.
            const string sql = @"
                SELECT obs.ObservationId, obs.ControllerId, ctrl.FullName AS ControllerName,
                       obs.ObservationNo, obs.Duration_Days, obs.TravelCountry,
                       obs.DepartDate, obs.ReturnDate, obs.LicenseNumber, obs.FilePath, obs.Notes, obs.EmployeeId
                FROM Observations obs
                LEFT JOIN Controllers ctrl ON obs.ControllerId = ctrl.ControllerId
                ORDER BY obs.DepartDate DESC";
            return ExecuteQuery(sql);
        }

        // GetAllObservationsbyDashboard: SQL is standard, no changes needed
        public DataTable GetAllObservationsbyDashboard()
        {
            const string sql = "SELECT * FROM observations";
            return ExecuteQuery(sql);
        }

        // GetObservationsByController: Parameter syntax change (@ instead of :), direct execution converted
        public DataTable GetObservationsByController(int controllerId)
        {
            const string sql = @"
SELECT observationid, controllerid, observationno, travelcount, duration_days,
       travelcountry, departdate, returndate, licensenumber, filepath, notes
FROM observations
WHERE controllerid = @controllerid -- Changed : to @
ORDER BY departdate DESC";
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn); // Use SqlCommand
            cmd.Parameters.Add(new SqlParameter("@controllerid", controllerId)); // Use SqlParameter and @
            using var adapter = new SqlDataAdapter(cmd); // Use SqlDataAdapter
            var dt = new DataTable();
            try
            {
                adapter.Fill(dt);
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Server Error in GetObservationsByController: {ex.Message}");
                throw;
            }
            return dt;
        }

        // CreateObservation: Parameter syntax change (@ instead of :), Sequence replacement (assuming IDENTITY)
        public int CreateObservation(Observation obs)
        {
            const string sql = @"
                    INSERT INTO observations
                        (controllerid, observationno, travelcount, duration_days, travelcountry,
                        departdate, returndate, licensenumber, filepath, notes)
                    VALUES
                        (@ctrlId, @obsNo, @travelCount, @duration, @country,
                        @depart, @return, @licenseNo, @path, @notes)";

            return ExecuteNonQuery(sql,
                new SqlParameter("@ctrlId", obs.ControllerId), // ??? ????? ?? ???? null
                new SqlParameter("@obsNo", (object?)obs.ObservationNo ?? DBNull.Value),
                //new SqlParameter("@travelCount", (object?)obs.TravelCount ?? DBNull.Value),
                new SqlParameter("@duration", (object?)obs.DurationDays ?? DBNull.Value),
                new SqlParameter("@country", (object?)obs.TravelCountry ?? DBNull.Value),
                new SqlParameter("@depart", (object?)obs.DepartDate ?? DBNull.Value),
                new SqlParameter("@return", (object?)obs.ReturnDate ?? DBNull.Value),
                new SqlParameter("@licenseNo", (object?)obs.LicenseNumber ?? DBNull.Value),
                new SqlParameter("@path", (object?)obs.FilePath ?? DBNull.Value),
                new SqlParameter("@notes", (object?)obs.Notes ?? DBNull.Value)
            );
        }

        // UpdateObservation: Parameter syntax change (@ instead of :)
        public int UpdateObservation(Observation obs)
        {
            const string sql = @"
                        UPDATE observations
                        SET duration_days = @duration, travelcountry = @country, departdate = @depart,
                            returndate = @return, licensenumber = @licenseNo, filepath = @path, notes = @notes
                        WHERE observationid = @obsId"; // Changed : to @
            return ExecuteNonQuery(sql,
                new SqlParameter("@duration", obs.DurationDays),
                new SqlParameter("@country", obs.TravelCountry),
                new SqlParameter("@depart", obs.DepartDate),
                new SqlParameter("@return", obs.ReturnDate),
                new SqlParameter("@licenseNo", obs.LicenseNumber),
                new SqlParameter("@path", obs.FilePath ?? (object)DBNull.Value),
                new SqlParameter("@notes", obs.Notes ?? (object)DBNull.Value),
                new SqlParameter("@obsId", obs.ObservationId)
            );
        }

        // DeleteObservation: Parameter syntax change (@ instead of :)
        public int DeleteObservation(int observationId)
        {
            const string sql = "DELETE FROM observations WHERE observationid = @obsId"; // Changed : to @
            return ExecuteNonQuery(sql, new SqlParameter("@obsId", observationId)); // Use SqlParameter and @
        }

        // CreateCertificate: Parameter syntax change (@ instead of :), Sequence replacement (assuming IDENTITY)
        public void CreateCertificate(Certificate cert)
        {
            // Assuming certificateid is an IDENTITY column in SQL Server
            const string sql = @"
INSERT INTO certificates
  (controllerid, typeid, certificatetitle, issuingauthority, issuingcountry,
   issuedate, expirydate, status, statusreason, filepath, notes)
VALUES
  (@controllerid, @typeId, @certificatetitle, @issuingauthority, @issuingcountry,
   @issueDate, @expiryDate, @status, @statusReason, @filePath, @notes)"; // Removed certificateid, changed : to @
            ExecuteNonQuery(sql,
                new SqlParameter("@controllerid", cert.ControllerId),
                new SqlParameter("@typeId", cert.TypeId),
                new SqlParameter("@certificatetitle", cert.CertificateTitle), // Use certificatetitle
                new SqlParameter("@issuingauthority", cert.IssuingAuthority ?? (object)DBNull.Value),
                new SqlParameter("@issuingcountry", cert.IssuingCountry ?? (object)DBNull.Value),
                new SqlParameter("@issueDate", cert.IssueDate),
                new SqlParameter("@expiryDate", cert.ExpiryDate),
                new SqlParameter("@status", cert.Status),
                new SqlParameter("@statusReason", cert.StatusReason ?? (object)DBNull.Value),
                new SqlParameter("@filePath", cert.FilePath ?? (object)DBNull.Value),
                new SqlParameter("@notes", cert.Notes ?? (object)DBNull.Value)
            );
        }

        // UpdateCertificate: Parameter syntax change (@ instead of :)
        public void UpdateCertificate(Certificate c)
        {
            const string sql = @"
UPDATE certificates SET
    controllerid = @controllerId, typeid = @typeId, certificatetitle = @certificatetitle, -- Use certificatetitle
    issuingauthority = @issuingAuthority, issuingcountry = @issuingCountry,
    issuedate = @issueDate, expirydate = @expiryDate, status = @status,
    statusreason = @statusReason, filepath = @filePath, notes = @notes
WHERE certificateid = @certificateId"; // Changed : to @
            // System.Diagnostics.Debug.WriteLine("-- UpdateCertificate SQL --\n" + sql);
            ExecuteNonQuery(sql,
                new SqlParameter("@controllerId", c.ControllerId),
                new SqlParameter("@typeId", c.TypeId),
                new SqlParameter("@certificatetitle", c.CertificateTitle), // Use certificatetitle
                new SqlParameter("@issuingAuthority", (object?)c.IssuingAuthority ?? DBNull.Value),
                new SqlParameter("@issuingCountry", (object?)c.IssuingCountry ?? DBNull.Value),
                new SqlParameter("@issueDate", c.IssueDate),
                new SqlParameter("@expiryDate", c.ExpiryDate),
                new SqlParameter("@status", c.Status),
                new SqlParameter("@statusReason", (object?)c.StatusReason ?? DBNull.Value),
                new SqlParameter("@filePath", (object?)c.FilePath ?? DBNull.Value),
                new SqlParameter("@notes", (object?)c.Notes ?? DBNull.Value),
                new SqlParameter("@certificateId", c.CertificateId)
            );
        }


        //        public DataRow GetControllerByUsername(string username)
        //        {
        //            const string sql = @"
        //SELECT *
        //FROM controllers
        //WHERE username = @username"; // Changed : to @
        //            var dt = ExecuteQuery(sql, new SqlParameter("@username", username)); // Use SqlParameter and @
        //            if (dt.Rows.Count > 0)
        //                return dt.Rows[0];
        //            return null;
        //        }
        public DataRow GetControllerByUsername(string username)
        {
            const string sql = "SELECT * FROM controllers WHERE username = @username";
            var dt = ExecuteQuery(sql, new SqlParameter("@username", username));
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }


        // GetLicensesByController (string username): Parameter syntax change (@ instead of :)
        public List<License> GetLicensesByController(string username)
        {
            const string sql = @"
                SELECT l.*, l.licensetype 
                FROM licenses l
                JOIN controllers c ON l.controllerid = c.controllerid
                WHERE c.username = @username ORDER BY l.expirydate ASC";
            var dt = ExecuteQuery(sql, new SqlParameter("@username", username));
            var list = new List<License>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new License
                {
                    LicenseId = Convert.ToInt32(row["licenseid"]),
                    TypeName = row["licensetype"].ToString(),
                    // FIX: Use the safe 'as' operator for nullable types
                    ExpiryDate = row["expirydate"] as DateTime?,
                    IssueDate = row["IssueDate"] as DateTime?,
                    FilePath = row["pdfpath"]?.ToString(),
                });
            }
            return list;
        }

        // GetCertificatesByController (string username): Parameter syntax change (@ instead of :)
        public List<CertificateViewModel> GetCertificatesByController(string username)
        {
            const string sql = @"
                SELECT cer.*, t.typename 
                FROM certificates cer
                JOIN controllers c ON cer.controllerid = c.controllerid
                JOIN documenttypes t ON cer.typeid = t.typeid
                WHERE c.username = @username ORDER BY cer.certificateid ASC";
            var dt = ExecuteQuery(sql, new SqlParameter("@username", username));
            var list = new List<CertificateViewModel>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new CertificateViewModel
                {
                    CertificateId = Convert.ToInt32(row["certificateid"]),
                    TypeName = row["typename"].ToString(),
                    Title = row["certificatetitle"].ToString(),
                    // FIX: Use the safe 'as' operator for nullable types
                    IssueDate = row["IssueDate"] != DBNull.Value ? Convert.ToDateTime(row["IssueDate"]) : default,
                    ExpiryDate = row["expirydate"] != DBNull.Value ? Convert.ToDateTime(row["expirydate"]) : default,
                    Status = row["status"].ToString(),
                    FilePath = row["filepath"]?.ToString(),
                });
            }
            return list;
        }

        // GetObservationsByController (string username): Parameter syntax change (@ instead of :)
        public List<Observation> GetObservationsByController(string username)
        {
            const string sql = @"
                SELECT o.* FROM observations o
                JOIN controllers c ON o.controllerid = c.controllerid
                WHERE c.username = @username ORDER BY o.departdate ASC";
            var dt = ExecuteQuery(sql, new SqlParameter("@username", username));
            var list = new List<Observation>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Observation
                {
                    ObservationId = Convert.ToInt32(row["observationid"]),
                    TravelCountry = row["travelcountry"]?.ToString(),
                    LicenseNumber = row["licensenumber"]?.ToString(),
                    DurationDays = row["duration_days"] as int?,
                    // FIX: Use the safe 'as' operator for nullable types
                    DepartDate = row["departdate"] as DateTime?,
                    ReturnDate = row["returndate"] as DateTime?,
                    Notes = row["notes"]?.ToString(),
                });
            }
            return list;
        }

        // --- Dashboard Section --- //

        // GetControllersCount: SQL is standard
        public int GetControllersCount()
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM controllers");
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        // GetExpiredLicensesCount: SYSDATE -> GETDATE()
        public int GetExpiredLicensesCount()
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM licenses WHERE expirydate < GETDATE()"); // Use GETDATE()
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        // GetSoonExpiringLicensesCount: SYSDATE -> GETDATE(), SYSDATE + 30 -> DATEADD
        public int GetSoonExpiringLicensesCount()
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM licenses WHERE expirydate BETWEEN GETDATE() AND DATEADD(day, 30, GETDATE())");
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        // GetCertificatesStats: SQL is standard
        public Dictionary<string, int> GetCertificatesStats()
        {
            var dt = ExecuteQuery(@"SELECT status, COUNT(*) cnt FROM certificates GROUP BY status");
            return dt.Rows.Cast<DataRow>().ToDictionary(r => r["status"].ToString(), r => Convert.ToInt32(r["cnt"]));
        }

        // GetTotalLicensesCount: SQL is standard
        public int GetTotalLicensesCount()
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM licenses");
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        // GetExpiredLicensesOverTime: TO_CHAR -> FORMAT, SYSDATE -> GETDATE()
        public Dictionary<string, int> GetExpiredLicensesOverTime()
        {
            // Using FORMAT (SQL Server 2012+)
            var dt = ExecuteQuery(@"
SELECT FORMAT(expirydate, 'yyyy-MM') AS expiry_month, COUNT(*) AS count
FROM licenses
WHERE expirydate < GETDATE() -- Use GETDATE()
GROUP BY FORMAT(expirydate, 'yyyy-MM') -- Use FORMAT
ORDER BY FORMAT(expirydate, 'yyyy-MM') -- Use FORMAT
");
            return dt.Rows.Cast<DataRow>().ToDictionary(r => r["expiry_month"].ToString(), r => Convert.ToInt32(r["count"]));
        }

        // GetControllerDetails: SQL is standard
        public List<ControllerUser> GetControllerDetails()
        {
            // ???? ??? ????? fullname ? username ?? ????? ????????
            var dt = ExecuteQuery(@"SELECT * FROM controllers");

            return dt.Rows.Cast<DataRow>().Select(r => new ControllerUser
            {
                FullName = r["fullname"].ToString(),
                Username = r["username"].ToString(),
                CurrentDepartment = r["Current_Department"].ToString(),
                EmploymentStatus = r["Employment_Status"].ToString(),
                JobTitle = r["Job_Title"].ToString(),
            }).ToList();
        }

        // GetExpiredLicensesDetails: SYSDATE -> GETDATE()
        public List<License> GetExpiredLicensesDetails()
        {
            var dt = ExecuteQuery(@"
SELECT l.licenseid, l.expirydate, c.fullname AS controllername, l.controllerid, l.LicenseType
FROM licenses l
JOIN controllers c ON l.controllerid = c.controllerid
WHERE l.expirydate < GETDATE() -- Use GETDATE()
");
            return dt.Rows.Cast<DataRow>().Select(r => new License
            {
                LicenseId = Convert.ToInt32(r["licenseid"]),
                ExpiryDate = Convert.ToDateTime(r["expirydate"]),
                ControllerName = r["controllername"].ToString(),
                LicenseType = r["LicenseType"].ToString(),// Assumes License model has ControllerName
                ControllerId = Convert.ToInt32(r["controllerid"]) // Assumes License model has ControllerId
            }).ToList();
        }

        // GetAllLicensesDetails: SQL is standard
        public List<License> GetAllLicensesDetails()
        {
            var dt = ExecuteQuery(@"
SELECT l.licenseid, l.expirydate, c.fullname AS controllername, l.controllerid, l.LicenseType
FROM licenses l
JOIN controllers c ON l.controllerid = c.controllerid
");
            return dt.Rows.Cast<DataRow>().Select(r => new License
            {
                LicenseId = Convert.ToInt32(r["licenseid"]),
                ExpiryDate = Convert.ToDateTime(r["expirydate"]),
                ControllerName = r["controllername"].ToString(),
                LicenseType = r["LicenseType"].ToString(),
                ControllerId = Convert.ToInt32(r["controllerid"])
            }).ToList();
        }


        public List<License> GetAllLicensesDetailsMix()
        {
            var allLicenses = new List<License>();
            allLicenses.AddRange(GetControllerLicenses());
            allLicenses.AddRange(GetEmployeeLicenses());
            return allLicenses.OrderBy(l => l.PersonName).ToList(); // ????? ??? ??? ?????
        }

        // GetSoonExpiringLicensesDetails: SYSDATE -> GETDATE(), SYSDATE + 30 -> DATEADD
        public List<License> GetSoonExpiringLicensesDetails()
        {
            var dt = ExecuteQuery(@"
SELECT l.licenseid, l.expirydate, c.fullname AS controllername, l.controllerid, l.LicenseType
FROM licenses l
JOIN controllers c ON l.controllerid = c.controllerid
WHERE l.expirydate BETWEEN GETDATE() AND DATEADD(day, 30, GETDATE()) -- Use GETDATE() and DATEADD
");
            return dt.Rows.Cast<DataRow>().Select(r => new License
            {
                LicenseId = Convert.ToInt32(r["licenseid"]),
                ExpiryDate = Convert.ToDateTime(r["expirydate"]),
                ControllerName = r["controllername"].ToString(),
                LicenseType = r["LicenseType"].ToString(),
                ControllerId = Convert.ToInt32(r["controllerid"])
            }).ToList();
        }

        // GetUserEmailById: Parameter syntax change (@ instead of :), direct execution converted
        public string GetUserEmailById(int userId)
        {
            using (SqlConnection connection = GetConnection()) // Use SqlConnection
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT email FROM CONTROLLERS WHERE controllerid = @userid", connection)) // Use SqlCommand, @userid
                {
                    command.Parameters.Add(new SqlParameter("@userid", userId)); // Use SqlParameter, @userid
                    using (SqlDataReader reader = command.ExecuteReader()) // Use SqlDataReader
                    {
                        if (reader.Read())
                        {
                            return reader["email"] == DBNull.Value ? null : reader["email"].ToString();
                        }
                        return null;
                    }
                }
            }
        }

        // --- Final Section with Manual Reader Loops (Converted) --- //

        // GetControllers: Parameter syntax change (@ instead of :), direct execution with reader loop converted
        public List<ControllerUser> GetControllers(
    string fullName, string username, string airportName, string icao_code,
    string jobTitle, string educationLevel, string maritalStatus,
    string phoneNumber, string email, string employmentStatus, string currentDepartment)
        {
            var controllers = new List<ControllerUser>();

            string query = @"SELECT c.controllerid, c.fullname, c.username, c.airportid, c.photopath, c.licensepath, c.userid, c.job_title,
                          c.education_level, c.date_of_birth, c.marital_status, c.phone_number, c.email, c.address, c.hire_date, 
                          c.employment_status, c.current_department, c.transfer_date, c.emergency_contact, c.LicenseNumber, 
                          a.airportname, a.icao_code, u.RoleName
                   FROM controllers c
                   JOIN airports a ON c.airportid = a.airportid
                   LEFT JOIN users u ON c.Username = u.Username
                   WHERE 1=1";

            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(fullName))
            {
                query += " AND LOWER(c.fullname) LIKE @fullName";
                parameters.Add(new SqlParameter("@fullName", $"%{fullName.ToLower()}%"));
            }
            if (!string.IsNullOrEmpty(username))
            {
                query += " AND LOWER(c.username) LIKE @username";
                parameters.Add(new SqlParameter("@username", $"%{username.ToLower()}%"));
            }
            if (!string.IsNullOrEmpty(airportName))
            {
                query += " AND LOWER(a.airportname) LIKE @airportName";
                parameters.Add(new SqlParameter("@airportName", $"%{airportName.ToLower()}%"));
            }
            if (!string.IsNullOrEmpty(jobTitle))
            {
                query += " AND LOWER(c.job_title) LIKE @jobTitle";
                parameters.Add(new SqlParameter("@jobTitle", $"%{jobTitle.ToLower()}%"));
            }
            if (!string.IsNullOrEmpty(employmentStatus))
            {
                query += " AND LOWER(c.employment_status) LIKE @employmentStatus";
                parameters.Add(new SqlParameter("@employmentStatus", $"%{employmentStatus.ToLower()}%"));
            }
            if (!string.IsNullOrEmpty(currentDepartment))
            {
                query += " AND LOWER(c.current_department) LIKE @currentDepartment";
                parameters.Add(new SqlParameter("@currentDepartment", $"%{currentDepartment.ToLower()}%"));
            }
            if (!string.IsNullOrEmpty(icao_code))
            {
                query += " AND LOWER(a.icao_code) LIKE @icao_code";
                parameters.Add(new SqlParameter("@icao_code", $"%{icao_code.ToLower()}%"));
            }
            if (!string.IsNullOrEmpty(educationLevel))
            {
                query += " AND LOWER(c.education_level) LIKE @educationLevel";
                parameters.Add(new SqlParameter("@educationLevel", $"%{educationLevel.ToLower()}%"));
            }
            if (!string.IsNullOrEmpty(maritalStatus))
            {
                query += " AND LOWER(c.marital_status) LIKE @maritalStatus";
                parameters.Add(new SqlParameter("@maritalStatus", $"%{maritalStatus.ToLower()}%"));
            }
            // =============================================================
            // ==> ?? ????? ????? ??? <==
            // =============================================================
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                query += " AND LOWER(c.phone_number) LIKE @phoneNumber";
                parameters.Add(new SqlParameter("@phoneNumber", $"%{phoneNumber.ToLower()}%"));
            }
            if (!string.IsNullOrEmpty(email))
            {
                query += " AND LOWER(c.email) LIKE @email";
                parameters.Add(new SqlParameter("@email", $"%{email.ToLower()}%"));
            }

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(query, conn))
                {
                    if (parameters.Any())
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            controllers.Add(new ControllerUser
                            {
                                ControllerId = Convert.ToInt32(reader["controllerid"]),
                                FullName = reader["fullname"].ToString(),
                                Username = reader["username"].ToString(),
                                AirportId = Convert.ToInt32(reader["airportid"]),
                                PhotoPath = reader["photopath"]?.ToString(),
                                LicensePath = reader["licensepath"]?.ToString(),
                                UserId = reader["userid"] != DBNull.Value ? Convert.ToInt32(reader["userid"]) : 0,
                                JobTitle = reader["job_title"]?.ToString(),
                                EducationLevel = reader["education_level"]?.ToString(),
                                DateOfBirth = reader["date_of_birth"] != DBNull.Value ? Convert.ToDateTime(reader["date_of_birth"]) : (DateTime?)null,
                                MaritalStatus = reader["marital_status"]?.ToString(),
                                PhoneNumber = reader["phone_number"]?.ToString(),
                                Email = reader["email"]?.ToString(),
                                Address = reader["address"]?.ToString(),
                                HireDate = reader["hire_date"] != DBNull.Value ? Convert.ToDateTime(reader["hire_date"]) : (DateTime?)null,
                                EmploymentStatus = reader["employment_status"]?.ToString(),
                                CurrentDepartment = reader["current_department"]?.ToString(),
                                TransferDate = reader["transfer_date"] != DBNull.Value ? Convert.ToDateTime(reader["transfer_date"]) : (DateTime?)null,
                                EmergencyContact = reader["emergency_contact"]?.ToString(),
                                LicenseNumber = reader["LicenseNumber"]?.ToString(),
                                AirportName = reader["airportname"]?.ToString(),
                                icao_code = reader["icao_code"]?.ToString(),
                                Role = reader["RoleName"]?.ToString()
                            });
                        }
                    }
                }
            }
            return controllers;
        }

        // GetLicenses: Parameter syntax change (@ instead of :), direct execution with reader loop converted
        public List<LicenseModel> GetLicenses(string filter)
        {
            var licenses = new List<LicenseModel>();
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = @"SELECT l.licenseid, l.controllerid, l.licensetype, l.expirydate, l.pdfpath, l.photopath, l.range, l.note, l.issuedate, l.licensenumber, c.fullname, c.username
                         FROM licenses l
                         JOIN controllers c ON l.controllerid = c.controllerid
                         WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    query += @" AND (LOWER(l.licensetype) LIKE @filter
                                OR LOWER(l.range) LIKE @filter
                                OR LOWER(l.note) LIKE @filter
                                OR LOWER(c.fullname) LIKE @filter
                                OR LOWER(c.username) LIKE @filter)"; // Changed : to @
                }

                using (var cmd = new SqlCommand(query, conn)) // Use SqlCommand
                {
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        cmd.Parameters.Add(new SqlParameter("@filter", $"%{filter.ToLower()}%")); // Use SqlParameter and @
                    }

                    using (var reader = cmd.ExecuteReader()) // Use SqlDataReader
                    {
                        while (reader.Read())
                        {
                            licenses.Add(new LicenseModel
                            {
                                LicenseId = reader["licenseid"].ToString(),
                                ControllerId = reader["controllerid"].ToString(),
                                LicenseType = reader["licensetype"].ToString(),
                                ExpiryDate = reader["expirydate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["expirydate"]),
                                PdfPath = reader["pdfpath"]?.ToString(),
                                PhotoPath = reader["photopath"]?.ToString(),
                                Range = reader["range"]?.ToString(),
                                Note = reader["note"]?.ToString(),
                                IssueDate = reader["issuedate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["issuedate"]),
                                licensenumber = reader["licensenumber"]?.ToString(), // Check model property name
                                FullName = reader["fullname"].ToString(),
                                Username = reader["username"].ToString()
                            });
                        }
                    }
                }
            }
            return licenses;
        }

        // GetCertificates: Parameter syntax change (@ instead of :), direct execution with reader loop converted
        public List<CertificateModel> GetCertificates(string filter)
        {
            var list = new List<CertificateModel>();
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = @"
SELECT s.certificateid, s.controllerid, c.fullname, d.typename, d.typeid, s.certificatetitle, s.issuingauthority,
       s.issuingcountry, s.issuedate, s.expirydate, s.status, s.statusreason, s.filepath, s.notes
FROM certificates s
JOIN controllers c ON s.controllerid = c.controllerid
JOIN documenttypes d ON s.typeid = d.typeid
WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    query += @"
AND (
    LOWER(c.fullname) LIKE @filter
    OR LOWER(d.typename) LIKE @filter
    OR LOWER(s.certificatetitle) LIKE @filter
    OR LOWER(s.issuingauthority) LIKE @filter
    OR LOWER(s.issuingcountry) LIKE @filter
)"; // Changed : to @
                }

                using (var cmd = new SqlCommand(query, conn)) // Use SqlCommand
                {
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        cmd.Parameters.Add(new SqlParameter("@filter", $"%{filter.ToLower()}%")); // Use SqlParameter and @
                    }

                    using (var reader = cmd.ExecuteReader()) // Use SqlDataReader
                    {
                        while (reader.Read())
                        {
                            list.Add(new CertificateModel
                            {
                                CertificateId = reader["certificateid"].ToString(),
                                ControllerId = reader["controllerid"].ToString(),
                                FullName = reader["fullname"].ToString(),
                                TypeName = reader["typename"].ToString(),
                                TypeId = reader["typeid"].ToString(),
                                CertificateTitle = reader["certificatetitle"].ToString(),
                                IssuingAuthority = reader["issuingauthority"]?.ToString(),
                                IssuingCountry = reader["issuingcountry"]?.ToString(),
                                IssueDate = reader["issuedate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["issuedate"]),
                                ExpiryDate = reader["expirydate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["expirydate"]),
                                Status = reader["status"].ToString(),
                                StatusReason = reader["statusreason"]?.ToString(),
                                FilePath = reader["filepath"]?.ToString(),
                                Notes = reader["notes"]?.ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        // GetObservations: Parameter syntax change (@ instead of :), direct execution with reader loop converted
        //        public List<Observation> GetObservations(string filter)
        //        {
        //            var list = new List<Observation>();
        //            using (var conn = GetConnection())
        //            {
        //                conn.Open();
        //                string query = @"
        //SELECT o.observationid, o.controllerid, c.fullname, o.travelcount, o.duration_days, o.travelcountry,
        //       o.departdate, o.returndate, o.licensenumber, o.filepath, o.observationno, o.notes
        //FROM observations o
        //JOIN controllers c ON o.controllerid = c.controllerid
        //WHERE 1=1";

        //                if (!string.IsNullOrWhiteSpace(filter))
        //                {
        //                    query += @"
        //AND (
        //    LOWER(c.fullname) LIKE @filter
        //    OR LOWER(o.travelcountry) LIKE @filter
        //    OR LOWER(o.licensenumber) LIKE @filter
        //    OR LOWER(o.notes) LIKE @filter
        //)"; // Changed : to @
        //                }

        //                using (var cmd = new SqlCommand(query, conn)) // Use SqlCommand
        //                {
        //                    if (!string.IsNullOrWhiteSpace(filter))
        //                    {
        //                        cmd.Parameters.Add(new SqlParameter("@filter", $"%{filter.ToLower()}%")); // Use SqlParameter and @
        //                    }

        //                    using (var reader = cmd.ExecuteReader()) // Use SqlDataReader
        //                    {
        //                        while (reader.Read())
        //                        {
        //                            list.Add(new Observation
        //                            {
        //                                ObservationId = reader["observationid"].ToString(),
        //                                ControllerId = reader["controllerid"].ToString(),
        //                                FullName = reader["fullname"].ToString(),
        //                                TravelCount = reader["travelcount"].ToString(),
        //                                DurationDays = reader["duration_days"].ToString(),
        //                                TravelCountry = reader["travelcountry"].ToString(),
        //                                DepartDate = reader["departdate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["departdate"]),
        //                                ReturnDate = reader["returndate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["returndate"]),
        //                                LicenseNumber = reader["licensenumber"]?.ToString(),
        //                                FilePath = reader["filepath"]?.ToString(),
        //                                ObservationNo = reader["observationno"]?.ToString(),
        //                                Notes = reader["notes"]?.ToString()
        //                            });
        //                        }
        //                    }
        //                }
        //            }
        //            return list;
        //        }

        // GetNotifications: Parameter syntax change (@ instead of :), direct execution with reader loop converted
        public List<NotificationModel> GetNotifications(string filter)
        {
            var list = new List<NotificationModel>();
            using (var conn = GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT 
                        n.NotificationId, 
                        n.userid, 
                        n.controllerid, 
                        COALESCE(c.fullname, e.fullname) as fullname,
                        n.message, 
                        n.link, 
                        n.created_at,
                        n.is_read, 
                        n.note, 
                        n.licensetype, 
                        n.licenseexpirydate, 
                        COALESCE(c.phone_number, e.phonenumber) as phone_number,
                        COALESCE(c.email, e.email) as email,
                        COALESCE(c.current_department, e.department) as current_department,
                        COALESCE(a.airportname, 'HQ - Main Office') as airportname
                    FROM notifications n
                    LEFT JOIN controllers c ON n.controllerid = c.controllerid
                    LEFT JOIN employees e ON n.userid = e.userid AND n.controllerid IS NULL
                    LEFT JOIN airports a ON c.airportid = a.airportid
                    WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    query += @" AND (LOWER(n.message) LIKE @filter
                          OR LOWER(n.note) LIKE @filter
                          OR LOWER(n.userid) LIKE @filter
                          OR LOWER(COALESCE(c.fullname, e.fullname)) LIKE @filter
                          OR LOWER(n.licensetype) LIKE @filter
                          OR LOWER(COALESCE(c.current_department, e.department)) LIKE @filter)";
                }

                using (var cmd = new SqlCommand(query, conn)) // Use SqlCommand
                {
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        cmd.Parameters.Add(new SqlParameter("@filter", $"%{filter.ToLower()}%")); // Use SqlParameter and @
                    }

                    using (var reader = cmd.ExecuteReader()) // Use SqlDataReader
                    {
                        while (reader.Read())
                        {
                            list.Add(new NotificationModel
                            {
                                NotificationId = reader["NotificationId"].ToString(),
                                UserId = reader["userid"]?.ToString(),
                                ControllerId = reader["controllerid"]?.ToString(),
                                FullName = reader["fullname"]?.ToString(),
                                Message = reader["message"]?.ToString(),
                                Link = reader["link"]?.ToString(),
                                CreatedAt = reader["created_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["created_at"]),
                                // Assuming is_read is bit/int (0/1) in SQL Server
                                IsRead = reader["is_read"] != DBNull.Value && (Convert.ToInt32(reader["is_read"]) == 1),
                                Note = reader["note"]?.ToString(),
                                LicenseType = reader["licensetype"]?.ToString(),
                                LicenseExpiryDate = reader["licenseexpirydate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["licenseexpirydate"]),
                                phonenumber = reader["phone_number"]?.ToString(), // Check model property name
                                Email = reader["email"]?.ToString(),
                                Currentdepartment = reader["current_department"]?.ToString(), // Check model property name
                                Location = reader["airportname"]?.ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        // GetSoonExpiringLicensesTable: SYSDATE -> GETDATE(), SYSDATE + 30 -> DATEADD, direct execution converted
        public DataTable GetSoonExpiringLicensesTable()
        {
            using (var connection = GetConnection()) // Use SqlConnection
            {
                connection.Open();
                using (var cmd = new SqlCommand(@"
SELECT c.fullname, l.licensetype, l.expirydate, c.phone_number, c.email
FROM licenses l
JOIN controllers c ON l.controllerid = c.controllerid
WHERE l.expirydate BETWEEN GETDATE() AND DATEADD(day, 30, GETDATE()) -- Use GETDATE() and DATEADD
", connection)) // Use SqlCommand
                using (var adapter = new SqlDataAdapter(cmd)) // Use SqlDataAdapter
                {
                    DataTable dt = new DataTable();
                    try
                    {
                        adapter.Fill(dt);
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine($"SQL Server Error in GetSoonExpiringLicensesTable: {ex.Message}");
                        throw;
                    }
                    return dt;
                }
            }
        }

        // ???? ????? ???? ??? ???? ?????? ????? ??? UserId
        public string GetUserPasswordHashByUserId(int userId)
        {
            string sql = "SELECT username, passwordhash FROM users WHERE userid = @userId";
            DataTable dt = ExecuteQuery(sql, new SqlParameter("@userId", userId));
            if (dt.Rows.Count > 0 && dt.Rows[0]["passwordhash"] != DBNull.Value)
            {
                return dt.Rows[0]["passwordhash"].ToString();
            }
            return null; // ?? ??? ??????? ??? ??? ???????? ??? ?????
        }




        // ========== ???? ????? ???? ????????? (????? ???? ???? SqlServerDb) ==========

        /// <summary>
        /// ???? ???? ???????? ?? ??????? ???????.
        /// </summary>
        public List<Employee> GetEmployees(
     string? fullName, string? employeeOfficialID, string? jobTitle,
     string? department, string? username, string? phoneNumber,
     string? email, string? location, string? Gender)
        {
            var employees = new List<Employee>();

            string sql = @"
        SELECT e.*, u.Username, e.Gender 
        FROM Employees e
        LEFT JOIN Users u ON e.UserID = u.UserID
        WHERE 1=1 ";

            var parameters = new List<SqlParameter>();

            // ???? ???? WHERE ???? ????????
            if (!string.IsNullOrEmpty(fullName))
            {
                sql += " AND LOWER(e.FullName) LIKE @fullName";
                parameters.Add(new SqlParameter("@fullName", $"%{fullName.ToLower()}%"));
            }
            if (!string.IsNullOrEmpty(employeeOfficialID))
            {
                sql += " AND LOWER(e.EmployeeOfficialID) LIKE @employeeOfficialID";
                parameters.Add(new SqlParameter("@employeeOfficialID", $"%{employeeOfficialID.ToLower()}%"));
            }
            if (!string.IsNullOrEmpty(jobTitle))
            {
                sql += " AND LOWER(e.JobTitle) LIKE @jobTitle";
                parameters.Add(new SqlParameter("@jobTitle", $"%{jobTitle.ToLower()}%"));
            }
            if (!string.IsNullOrEmpty(department))
            {
                sql += " AND LOWER(e.Department) LIKE @department";
                parameters.Add(new SqlParameter("@department", $"%{department.ToLower()}%"));
            }
            if (!string.IsNullOrEmpty(username))
            {
                sql += " AND LOWER(u.Username) LIKE @username";
                parameters.Add(new SqlParameter("@username", $"%{username.ToLower()}%"));
            }
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                sql += " AND e.PhoneNumber LIKE @phoneNumber";
                parameters.Add(new SqlParameter("@phoneNumber", $"%{phoneNumber}%"));
            }
            if (!string.IsNullOrEmpty(email))
            {
                sql += " AND LOWER(e.Email) LIKE @email";
                parameters.Add(new SqlParameter("@email", $"%{email.ToLower()}%"));
            }
            if (!string.IsNullOrEmpty(location))
            {
                sql += " AND LOWER(e.Location) LIKE @location";
                parameters.Add(new SqlParameter("@location", $"%{location.ToLower()}%"));
            }
            if (!string.IsNullOrEmpty(Gender))
            {
                sql += " AND LOWER(e.Location) LIKE @Gender";
                parameters.Add(new SqlParameter("@Gender", $"%{Gender.ToLower()}%"));
            }
            sql += " ORDER BY e.FullName";

            DataTable dt = ExecuteQuery(sql, parameters.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                employees.Add(MapDataRowToEmployee(row));
            }
            return employees;
        }
        /// <summary>
        /// ???? ???? ?????? ???? ???? ???????? ???? ????????.
        /// </summary>
        /// <param name="employeeId">????? ???????? ??????</param>
        /// <returns>???? ?? ??? ???? ?? null ??? ?? ??? ?????? ????</returns>
        public Employee GetEmployeeById(int employeeId)
        {
            // ??? ????????? ????? ??? ???? ?? ???? ?????????? ???? ??? ????????
            string sql = @"
        SELECT e.*, u.Username 
        FROM Employees e
        LEFT JOIN Users u ON e.UserID = u.UserID
        WHERE e.EmployeeID = @EmployeeID";

            var parameters = new[] { new SqlParameter("@EmployeeID", employeeId) };
            DataTable dt = ExecuteQuery(sql, parameters);

            if (dt.Rows.Count > 0)
            {
                 DataRow row = dt.Rows[0];
                //return new Employee
                //{
                //    EmployeeID = Convert.ToInt32(row["EmployeeId"]),
                //    FullName = row["FullName"] != DBNull.Value ? row["FullName"].ToString() : string.Empty,
                //    Department = row["Department"] != DBNull.Value ? row["Department"].ToString() : string.Empty,
                //    // ??? ???? ?????? ??? ??????
                //};
                return MapDataRowToEmployee(dt.Rows[0]);
            }
            return null;
        }

        /// <summary>
        /// ???? ?????? ?????? ???? ?? ???? ???? ??????? ???? ?? ????? ?????.
        /// </summary>
        /// <param name="model">???????? ??????? ?? ???? ???????</param>
        public void CreateEmployeeAndUser(CreateEmployeeViewModel model)
        {
            // ?????? (?): ???? ???? ??? ????? ??????
            var passwordHasher = new PasswordHasher<ControllerUser>();
            string hashedPassword = passwordHasher.HashPassword(null, model.Password);

            // ?????? (?): ?????? ???? CreateUser ???????? ???? ?????? ??????
            int newUserId = CreateUser(model.Username, hashedPassword, model.RoleName);

            if (newUserId <= 0)
            {
                // ??? ???? ????? ????? ????????? ?? ???? ????? ???
                throw new Exception("Failed to create the user account for the employee.");
            }

            // ?????? (?): ???? ???? ?????? ?????? ?? ???? ???? UserID ??????
            string sql = @"
        INSERT INTO Employees (
            EmployeeOfficialID, UserID, FullName, JobTitle, Department, 
            PhoneNumber, Email, HireDate, IsActive, Address, Location, EmergencyContactPhone, Gender,
            DateOfBirth, MaritalStatus, EducationLevel,
            CurrentSalary, AnnualIncreasePercentage, SalaryAfterAnnualIncrease,
            BankAccountNumber, BankName, TaxId, InsuranceNumber
        ) VALUES (
            @EmployeeOfficialID, @UserID, @FullName, @JobTitle, @Department, 
            @PhoneNumber, @Email, @HireDate, @IsActive, @Address, @Location, @EmergencyContactPhone, @Gender,
            @DateOfBirth, @MaritalStatus, @EducationLevel,
            @CurrentSalary, @AnnualIncreasePercentage, @SalaryAfterAnnualIncrease,
            @BankAccountNumber, @BankName, @TaxId, @InsuranceNumber
        )";

            var parameters = new[]
            {
        new SqlParameter("@EmployeeOfficialID", model.EmployeeOfficialID),
        new SqlParameter("@UserID", newUserId),
        new SqlParameter("@FullName", model.FullName),
        new SqlParameter("@JobTitle", (object)model.JobTitle ?? DBNull.Value),
        new SqlParameter("@Department", (object)model.Department ?? DBNull.Value),
        new SqlParameter("@PhoneNumber", (object)model.PhoneNumber ?? DBNull.Value),
        new SqlParameter("@Email", model.Email),
        new SqlParameter("@HireDate", (object)model.HireDate ?? DBNull.Value),
        new SqlParameter("@IsActive", model.IsActive),
        new SqlParameter("@Address", (object)model.Address ?? DBNull.Value),
        new SqlParameter("@Location", (object)model.Location ?? DBNull.Value),
        new SqlParameter("@EmergencyContactPhone", (object)model.EmergencyContactPhone ?? DBNull.Value),
        new SqlParameter("@Gender", (object)model.Gender ?? DBNull.Value),
        new SqlParameter("@DateOfBirth", (object)model.DateOfBirth ?? DBNull.Value),
        new SqlParameter("@MaritalStatus", (object)model.MaritalStatus ?? DBNull.Value),
        new SqlParameter("@EducationLevel", (object)model.EducationLevel ?? DBNull.Value),
        new SqlParameter("@CurrentSalary", (object)model.CurrentSalary ?? DBNull.Value),
        new SqlParameter("@AnnualIncreasePercentage", (object)model.AnnualIncreasePercentage ?? DBNull.Value),
        new SqlParameter("@SalaryAfterAnnualIncrease", (object)model.SalaryAfterAnnualIncrease ?? DBNull.Value),
        new SqlParameter("@BankAccountNumber", (object)model.BankAccountNumber ?? DBNull.Value),
        new SqlParameter("@BankName", (object)model.BankName ?? DBNull.Value),
        new SqlParameter("@TaxId", (object)model.TaxId ?? DBNull.Value),
        new SqlParameter("@InsuranceNumber", (object)model.InsuranceNumber ?? DBNull.Value),
        new SqlParameter("@PhotoPath", (object)model.PhotoPath ?? DBNull.Value),
        new SqlParameter("@NeedLicense", model.NeedLicense),
    };

            ExecuteNonQuery(sql, parameters);
        }


        /// <summary>
        /// ???? ?????? ?????? ???? ?????.
        /// </summary>
        /// <param name="employee">???? ?????? ????? ??? ???????? ???????</param>
        public void UpdateEmployee(Employee employee)
        {
            string sql = @"
        UPDATE Employees SET
            EmployeeOfficialID = @EmployeeOfficialID,
            FullName = @FullName,
            JobTitle = @JobTitle,
            Department = @Department,
            PhoneNumber = @PhoneNumber,
            Email = @Email,
            HireDate = @HireDate,
            IsActive = @IsActive,
            Address = @Address,
            Location = @Location,
            EmergencyContactPhone = @EmergencyContactPhone,
            Gender = @Gender,
            DateOfBirth = @DateOfBirth,
            MaritalStatus = @MaritalStatus,
            EducationLevel = @EducationLevel,
            CurrentSalary = @CurrentSalary,
            AnnualIncreasePercentage = @AnnualIncreasePercentage,
            SalaryAfterAnnualIncrease = @SalaryAfterAnnualIncrease,
            BankAccountNumber = @BankAccountNumber,
            BankName = @BankName,
            TaxId = @TaxId,
            InsuranceNumber = @InsuranceNumber,
            PhotoPath = @PhotoPath,
            NeedLicense = @NeedLicense
        WHERE EmployeeID = @EmployeeID";

            var parameters = new[]
            {
        new SqlParameter("@EmployeeOfficialID", employee.EmployeeOfficialID),
        new SqlParameter("@FullName", employee.FullName),
        new SqlParameter("@JobTitle", (object)employee.JobTitle ?? DBNull.Value),
        new SqlParameter("@Department", (object)employee.Department ?? DBNull.Value),
        new SqlParameter("@PhoneNumber", (object)employee.PhoneNumber ?? DBNull.Value),
        new SqlParameter("@Email", employee.Email),
        new SqlParameter("@HireDate", (object)employee.HireDate ?? DBNull.Value),
        new SqlParameter("@IsActive", employee.IsActive),
        new SqlParameter("@Address", (object)employee.Address ?? DBNull.Value),
        new SqlParameter("@Location", (object)employee.Location ?? DBNull.Value),
        new SqlParameter("@EmergencyContactPhone", (object)employee.EmergencyContactPhone ?? DBNull.Value),
        new SqlParameter("@Gender", (object)employee.Gender ?? DBNull.Value),
        new SqlParameter("@DateOfBirth", (object)employee.DateOfBirth ?? DBNull.Value),
        new SqlParameter("@MaritalStatus", (object)employee.MaritalStatus ?? DBNull.Value),
        new SqlParameter("@EducationLevel", (object)employee.EducationLevel ?? DBNull.Value),
        new SqlParameter("@CurrentSalary", (object)employee.CurrentSalary ?? DBNull.Value),
        new SqlParameter("@AnnualIncreasePercentage", (object)employee.AnnualIncreasePercentage ?? DBNull.Value),
        new SqlParameter("@SalaryAfterAnnualIncrease", (object)employee.SalaryAfterAnnualIncrease ?? DBNull.Value),
        new SqlParameter("@BankAccountNumber", (object)employee.BankAccountNumber ?? DBNull.Value),
        new SqlParameter("@BankName", (object)employee.BankName ?? DBNull.Value),
        new SqlParameter("@TaxId", (object)employee.TaxId ?? DBNull.Value),
        new SqlParameter("@InsuranceNumber", (object)employee.InsuranceNumber ?? DBNull.Value),
        new SqlParameter("@EmployeeID", employee.EmployeeID)
    };
            ExecuteNonQuery(sql, parameters);
        }

        /// <summary>
        /// ???? ?????? ???? ?????? ?? ?????? ?? DataTable ??? ???? Employee.
        /// ??? ???? ????? ????? ????? ???????.
        /// </summary>
        /// <param name="row">?? ???????? ?????? ??????</param>
        /// <returns>???? ????</returns>
        //private Employee MapDataRowToEmployee(DataRow row)
        //{
        //    return new Employee
        //    {
        //        EmployeeID = Convert.ToInt32(row["EmployeeID"]),
        //        EmployeeOfficialID = row["EmployeeOfficialID"]?.ToString(),
        //        UserID = row["UserID"] as int?,
        //        FullName = row["FullName"]?.ToString(),
        //        JobTitle = row["JobTitle"]?.ToString(),
        //        Department = row["Department"]?.ToString(),
        //        PhoneNumber = row["PhoneNumber"]?.ToString(),
        //        Email = row["Email"]?.ToString(),
        //        HireDate = row["HireDate"] as DateTime?,
        //        TerminationDate = row["TerminationDate"] as DateTime?,
        //        IsActive = Convert.ToBoolean(row["IsActive"]),
        //        Address = row["Address"]?.ToString(),
        //        Location = row["Location"]?.ToString(),
        //        EmergencyContactPhone = row["EmergencyContactPhone"]?.ToString(),
        //        Gender = row["Gender"]?.ToString(),
        //        Username = row["Username"]?.ToString() // From the JOIN with Users table
        //    };
        //}



        public bool EmployeeEmailExists(string email)
        {
            string sql = "SELECT 1 FROM Employees WHERE Email = @Email";
            var parameters = new[] { new SqlParameter("@Email", email) };

            object result = ExecuteScalar(sql, parameters);

            // ??? ????? ????? ???????? ?? ???? (??? ?? ???? 1)? ???? ???? ?? ??????? ?????
            return result != null;
        }
        /// <summary>
        /// ???? ???? ?? ????????? ??????? ?? ???? Roles
        /// </summary>
        public List<Role> GetAllRoles()
        {
            var roles = new List<Role>();
            // ???? ?? ?? ??? ?????? ??????? ????
            string sql = "SELECT RoleName FROM dbo.Roles ORDER BY RoleName";

            DataTable dt = ExecuteQuery(sql);

            foreach (DataRow row in dt.Rows)
            {
                roles.Add(new Role { RoleName = row["RoleName"].ToString()! });
            }
            return roles;
        }

        // DeleteEmployee: Parameter syntax change (@ instead of :)
        public void DeleteEmployee(int employeeId)
        {
            // ?????? ???? ??? ????? ???????? (UserID) ??????? ??????? ??? ????
            string getUserIdSql = "SELECT UserID FROM Employees WHERE EmployeeID = @EmployeeID";
            var getUserIdParams = new[] { new SqlParameter("@EmployeeID", employeeId) };
            DataTable dt = ExecuteQuery(getUserIdSql, getUserIdParams);

            int? userId = null;
            if (dt.Rows.Count > 0 && dt.Rows[0]["UserID"] != DBNull.Value)
            {
                userId = Convert.ToInt32(dt.Rows[0]["UserID"]);
            }

            // ??????? ???? ??? ?????? ?? ???? ????????
            string deleteEmployeeSql = "DELETE FROM Employees WHERE EmployeeID = @EmployeeID";
            var deleteEmployeeParams = new[] { new SqlParameter("@EmployeeID", employeeId) };
            ExecuteNonQuery(deleteEmployeeSql, deleteEmployeeParams);

            // ??????? ??? ??? ???? ?????? ?????? ????? ?? ???? ??????????
            if (userId.HasValue)
            {
                string deleteUserSql = "DELETE FROM Users WHERE UserID = @UserID";
                var deleteUserParams = new[] { new SqlParameter("@UserID", userId.Value) };
                ExecuteNonQuery(deleteUserSql, deleteUserParams);
            }
        }


        /// <summary>

        /// <summary>
        /// ???? ?? ????? ???????? ?????????? ???????
        /// </summary>
        public List<License> GetControllerLicenses()
        {
            var licenses = new List<License>();
            const string sql = @"
        SELECT 
            l.LicenseId, l.ControllerId, l.LicenseType, l.IssueDate, l.ExpiryDate, 
            l.PDFPath, l.licensenumber, l.Note, l.RANGE,
            c.FullName AS ControllerName,
            c.current_department AS ControllerCurrentDepartment,
            a.AirportName,
            a.Icao_Code AS AirportIcao
        FROM 
            Licenses l
        JOIN 
            Controllers c ON l.ControllerId = c.ControllerId
        LEFT JOIN
            Airports a ON c.AirportId = a.AirportId
        WHERE
            l.EmployeeID IS NULL; -- ???? ??? ????????? ???
    ";

            DataTable dt = ExecuteQuery(sql);

            foreach (DataRow row in dt.Rows)
            {
                licenses.Add(new License
                {
                    LicenseId = Convert.ToInt32(row["LicenseId"]),
                    ControllerId = Convert.ToInt32(row["ControllerId"]),
                    LicenseType = row["LicenseType"]?.ToString(),
                    ExpiryDate = row["ExpiryDate"] as DateTime?,
                    IssueDate = row["IssueDate"] as DateTime?,
                    Note = row["Note"]?.ToString(),
                    PDFPath = row["PDFPath"]?.ToString(),
                    licensenumber = row["licensenumber"]?.ToString(),
                    RANGE = row["RANGE"]?.ToString(),
                    ControllerName = row["ControllerName"]?.ToString(),
                    ControllerCurrentDepartment = row["ControllerCurrentDepartment"]?.ToString(),
                    AirportName = row["AirportName"]?.ToString(),
                    AirportIcao = row["AirportIcao"]?.ToString(),
                });
            }
            return licenses;
        }
        /// <summary>
        /// ???? ?? ?????/????????? ???????? ?????????
        /// </summary>
        /// <summary>
        /// Fetches all licenses/permissions related to Employees.
        /// </summary>
        public List<License> GetEmployeeLicenses()
        {
            var licenses = new List<License>();
            // This query now includes the 'RANGE' column
            const string sql = @"
        SELECT 
            l.LICENSEID, l.LICENSETYPE, l.EXPIRYDATE, l.ISSUEDATE, l.NOTE, l.PDFPATH, l.LICENSENUMBER, l.RANGE,
            e.EmployeeID,
            e.FullName AS EmployeeName,
            e.Department AS EmployeeDepartment
        FROM 
            LICENSES l
        JOIN 
            Employees e ON l.EmployeeID = e.EmployeeID
        WHERE
            l.ControllerID IS NULL; -- To get only employee licenses
    ";

            DataTable dt = ExecuteQuery(sql);

            foreach (DataRow row in dt.Rows)
            {
                var license = new License
                {
                    LicenseId = Convert.ToInt32(row["LICENSEID"]),
                    EmployeeId = Convert.ToInt32(row["EmployeeID"]),
                    LicenseType = row["LICENSETYPE"]?.ToString(),
                    ExpiryDate = row["EXPIRYDATE"] as DateTime?,
                    IssueDate = row["ISSUEDATE"] as DateTime?,
                    Note = row["NOTE"]?.ToString(),
                    PDFPath = row["PDFPATH"]?.ToString(),
                    licensenumber = row["LICENSENUMBER"]?.ToString(),
                    RANGE = row["RANGE"]?.ToString(), // Now we are reading the RANGE value
                    EmployeeName = row["EmployeeName"]?.ToString(),
                    EmployeeDepartment = row["EmployeeDepartment"]?.ToString()
                };
                licenses.Add(license);
            }
            return licenses;
        }

        /// <summary>
        /// Fetches AIS licenses/permissions
        /// </summary>
        public List<License> GetAISLicenses()
        {
            var licenses = new List<License>();
            const string sql = @"
        SELECT 
            l.LICENSEID, l.LICENSETYPE, l.EXPIRYDATE, l.ISSUEDATE, l.NOTE, l.PDFPATH, l.LICENSENUMBER, l.RANGE,
            e.EmployeeID,
            e.FullName AS EmployeeName,
            e.Department AS EmployeeDepartment
        FROM 
            LICENSES l
        JOIN 
            Employees e ON l.EmployeeID = e.EmployeeID
        WHERE
            l.ControllerID IS NULL 
            AND e.Department LIKE '%AIS%';
    ";

            DataTable dt = ExecuteQuery(sql);

            foreach (DataRow row in dt.Rows)
            {
                var license = new License
                {
                    LicenseId = Convert.ToInt32(row["LICENSEID"]),
                    EmployeeId = Convert.ToInt32(row["EmployeeID"]),
                    LicenseType = row["LICENSETYPE"]?.ToString(),
                    ExpiryDate = row["EXPIRYDATE"] as DateTime?,
                    IssueDate = row["ISSUEDATE"] as DateTime?,
                    Note = row["NOTE"]?.ToString(),
                    PDFPath = row["PDFPATH"]?.ToString(),
                    licensenumber = row["LICENSENUMBER"]?.ToString(),
                    RANGE = row["RANGE"]?.ToString(),
                    EmployeeName = row["EmployeeName"]?.ToString(),
                    EmployeeDepartment = row["EmployeeDepartment"]?.ToString()
                };
                licenses.Add(license);
            }
            return licenses;
        }

        /// <summary>
        /// Fetches CNS licenses/permissions
        /// </summary>
        public List<License> GetCNSLicenses()
        {
            var licenses = new List<License>();
            const string sql = @"
        SELECT 
            l.LICENSEID, l.LICENSETYPE, l.EXPIRYDATE, l.ISSUEDATE, l.NOTE, l.PDFPATH, l.LICENSENUMBER, l.RANGE,
            e.EmployeeID,
            e.FullName AS EmployeeName,
            e.Department AS EmployeeDepartment
        FROM 
            LICENSES l
        JOIN 
            Employees e ON l.EmployeeID = e.EmployeeID
        WHERE
            l.ControllerID IS NULL 
            AND e.Department LIKE '%CNS%';
    ";

            DataTable dt = ExecuteQuery(sql);

            foreach (DataRow row in dt.Rows)
            {
                var license = new License
                {
                    LicenseId = Convert.ToInt32(row["LICENSEID"]),
                    EmployeeId = Convert.ToInt32(row["EmployeeID"]),
                    LicenseType = row["LICENSETYPE"]?.ToString(),
                    ExpiryDate = row["EXPIRYDATE"] as DateTime?,
                    IssueDate = row["ISSUEDATE"] as DateTime?,
                    Note = row["NOTE"]?.ToString(),
                    PDFPath = row["PDFPATH"]?.ToString(),
                    licensenumber = row["LICENSENUMBER"]?.ToString(),
                    RANGE = row["RANGE"]?.ToString(),
                    EmployeeName = row["EmployeeName"]?.ToString(),
                    EmployeeDepartment = row["EmployeeDepartment"]?.ToString()
                };
                licenses.Add(license);
            }
            return licenses;
        }

        /// <summary>
        /// Fetches AFTN licenses/permissions
        /// </summary>
        public List<License> GetAFTNLicenses()
        {
            var licenses = new List<License>();
            const string sql = @"
        SELECT 
            l.LICENSEID, l.LICENSETYPE, l.EXPIRYDATE, l.ISSUEDATE, l.NOTE, l.PDFPATH, l.LICENSENUMBER, l.RANGE,
            e.EmployeeID,
            e.FullName AS EmployeeName,
            e.Department AS EmployeeDepartment
        FROM 
            LICENSES l
        JOIN 
            Employees e ON l.EmployeeID = e.EmployeeID
        WHERE
            l.ControllerID IS NULL 
            AND e.Department LIKE '%AFTN%';
    ";

            DataTable dt = ExecuteQuery(sql);

            foreach (DataRow row in dt.Rows)
            {
                var license = new License
                {
                    LicenseId = Convert.ToInt32(row["LICENSEID"]),
                    EmployeeId = Convert.ToInt32(row["EmployeeID"]),
                    LicenseType = row["LICENSETYPE"]?.ToString(),
                    ExpiryDate = row["EXPIRYDATE"] as DateTime?,
                    IssueDate = row["ISSUEDATE"] as DateTime?,
                    Note = row["NOTE"]?.ToString(),
                    PDFPath = row["PDFPATH"]?.ToString(),
                    licensenumber = row["LICENSENUMBER"]?.ToString(),
                    RANGE = row["RANGE"]?.ToString(),
                    EmployeeName = row["EmployeeName"]?.ToString(),
                    EmployeeDepartment = row["EmployeeDepartment"]?.ToString()
                };
                licenses.Add(license);
            }
            return licenses;
        }

        /// <summary>
        /// Fetches Ops Staff & Administration licenses/permissions
        /// </summary>
        public List<License> GetOpsStaffLicenses()
        {
            var licenses = new List<License>();
            const string sql = @"
        SELECT 
            l.LICENSEID, l.LICENSETYPE, l.EXPIRYDATE, l.ISSUEDATE, l.NOTE, l.PDFPATH, l.LICENSENUMBER, l.RANGE,
            e.EmployeeID,
            e.FullName AS EmployeeName,
            e.Department AS EmployeeDepartment
        FROM 
            LICENSES l
        JOIN 
            Employees e ON l.EmployeeID = e.EmployeeID
        WHERE
            l.ControllerID IS NULL 
            AND (e.Department LIKE '%Administration%' OR e.Department LIKE '%Safety%' OR e.Department LIKE '%Quality%');
    ";

            DataTable dt = ExecuteQuery(sql);

            foreach (DataRow row in dt.Rows)
            {
                var license = new License
                {
                    LicenseId = Convert.ToInt32(row["LICENSEID"]),
                    EmployeeId = Convert.ToInt32(row["EmployeeID"]),
                    LicenseType = row["LICENSETYPE"]?.ToString(),
                    ExpiryDate = row["EXPIRYDATE"] as DateTime?,
                    IssueDate = row["ISSUEDATE"] as DateTime?,
                    Note = row["NOTE"]?.ToString(),
                    PDFPath = row["PDFPATH"]?.ToString(),
                    licensenumber = row["LICENSENUMBER"]?.ToString(),
                    RANGE = row["RANGE"]?.ToString(),
                    EmployeeName = row["EmployeeName"]?.ToString(),
                    EmployeeDepartment = row["EmployeeDepartment"]?.ToString()
                };
                licenses.Add(license);
            }
            return licenses;
        }


        /// <summary>
        /// ???? ?????? ???? ????? ????? ??? ????? ????????
        /// </summary>
        public License GetLicenseById(int licenseId)
        {
            const string sql = @"
SELECT 
    l.*,
    c.FullName AS ControllerName,
    c.current_department AS ControllerCurrentDepartment,
    a.AirportName,
    a.Icao_Code AS AirportIcao,
    e.FullName AS EmployeeName,
    e.Department AS EmployeeDepartment
FROM Licenses l
LEFT JOIN Controllers c ON l.ControllerId = c.ControllerId
LEFT JOIN Airports a ON c.AirportId = a.AirportId
LEFT JOIN Employees e ON l.EmployeeId = e.EmployeeId
WHERE l.LicenseId = @LicenseId";

            var parameters = new[] { new SqlParameter("@LicenseId", licenseId) };
            DataTable dt = ExecuteQuery(sql, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new License
                {
                    LicenseId = Convert.ToInt32(row["LicenseId"]),
                    // ??????? ????? ???? ?????? ??????? ?? ????? ???????? ???????
                    LicenseType = row["LicenseType"] != DBNull.Value ? row["LicenseType"].ToString() : string.Empty,
                    ExpiryDate = row["ExpiryDate"] != DBNull.Value ? Convert.ToDateTime(row["ExpiryDate"]) : null,
                    IssueDate = row["IssueDate"] != DBNull.Value ? Convert.ToDateTime(row["IssueDate"]) : null,
                    Note = row["Note"] != DBNull.Value ? row["Note"].ToString() : string.Empty,
                    PDFPath = row["PDFPath"] != DBNull.Value ? row["PDFPath"].ToString() : string.Empty,
                    licensenumber = row["licensenumber"] != DBNull.Value ? row["licensenumber"].ToString() : string.Empty,
                    RANGE = row["RANGE"] != DBNull.Value ? row["RANGE"].ToString() : string.Empty,

                    // ??????? ?? ????? ???????
                    ControllerId = row["ControllerId"] != DBNull.Value ? Convert.ToInt32(row["ControllerId"]) : null,

                    // ??????? ?? ??????
                    ControllerName = row["ControllerName"] != DBNull.Value ? row["ControllerName"].ToString() : string.Empty,
                    ControllerCurrentDepartment = row["ControllerCurrentDepartment"] != DBNull.Value ? row["ControllerCurrentDepartment"].ToString() : string.Empty,
                    AirportName = row["AirportName"] != DBNull.Value ? row["AirportName"].ToString() : string.Empty,
                    AirportIcao = row["AirportIcao"] != DBNull.Value ? row["AirportIcao"].ToString() : string.Empty,

                    // ??????? ?? ???? ??????
                    EmployeeId = row["EmployeeId"] != DBNull.Value ? Convert.ToInt32(row["EmployeeId"]) : null,
                    EmployeeName = row["EmployeeName"] != DBNull.Value ? row["EmployeeName"].ToString() : string.Empty,
                    EmployeeDepartment = row["EmployeeDepartment"] != DBNull.Value ? row["EmployeeDepartment"].ToString() : string.Empty,
                };
            }
            return null;
        }

        /// <summary>
        /// ???? ???? ????? ?? ????? ???????? ????? ??? ????? ????????
        /// </summary>
        public void DeleteLicenseById(int licenseId)
        {
            const string sql = "DELETE FROM Licenses WHERE LicenseId = @LicenseId";
            var parameters = new[] { new SqlParameter("@LicenseId", licenseId) };
            ExecuteNonQuery(sql, parameters);
        }

        // --- ADD THESE TWO NEW METHODS TO YOUR SqlServerDb.cs FILE ---
        // --- ?????? ????????? ??????? ?? ??? SqlServerDb.cs ???? ?????? ??????? ---

        /// <summary>
        /// ???? ?? ???????? ???????? ???????? ?????????? ???????
        /// </summary>
        public List<CertificateViewModel> GetControllerCertificates()
        {
            var certificates = new List<CertificateViewModel>();
            // *** ?? ????? ??? ?????? ??? DocumentTypes ***
            const string sql = @"
        SELECT 
            ce.CertificateId, 
            t.TypeName, 
            ce.CertificateTitle AS Title,
            ce.IssueDate, ce.ExpiryDate, ce.Status, ce.FilePath,
            c.ControllerId,
            c.FullName AS ControllerName
        FROM 
            Certificates ce
        JOIN 
            Controllers c ON ce.ControllerId = c.ControllerId
        LEFT JOIN
            DocumentTypes t ON ce.TypeId = t.TypeId -- Corrected table name
        WHERE
            ce.EmployeeId IS NULL;
    ";
            DataTable dt = ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                certificates.Add(new CertificateViewModel
                {
                    CertificateId = Convert.ToInt32(row["CertificateId"]),
                    TypeName = row["TypeName"].ToString(),
                    Title = row["Title"].ToString(),
                    IssueDate = Convert.ToDateTime(row["IssueDate"]),
                    ExpiryDate = Convert.ToDateTime(row["ExpiryDate"]),
                    Status = row["Status"].ToString(),
                    FilePath = row["FilePath"].ToString(),
                    ControllerId = Convert.ToInt32(row["ControllerId"]),
                    ControllerName = row["ControllerName"].ToString()
                });
            }
            return certificates;
        }

        /// <summary>
        /// ???? ?? ???????? ???????? ???????? ?????????
        /// </summary>
        public List<CertificateViewModel> GetEmployeeCertificates()
        {
            var certificates = new List<CertificateViewModel>();
            // *** ?? ????? ??? ?????? ??? DocumentTypes ***
            const string sql = @"
        SELECT 
            ce.CertificateId, 
            t.TypeName,
            ce.CertificateTitle AS Title,
            ce.IssueDate, ce.ExpiryDate, ce.Status, ce.FilePath,
            e.EmployeeID,
            e.FullName AS EmployeeName,
            e.Department AS EmployeeDepartment
        FROM 
            Certificates ce
        JOIN 
            Employees e ON ce.EmployeeId = e.EmployeeID
        LEFT JOIN
            DocumentTypes t ON ce.TypeId = t.TypeId -- Corrected table name
        WHERE
            ce.ControllerId IS NULL;
    ";
            DataTable dt = ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                certificates.Add(new CertificateViewModel
                {
                    CertificateId = Convert.ToInt32(row["CertificateId"]),
                    TypeName = row["TypeName"].ToString(),
                    Title = row["Title"].ToString(),
                    IssueDate = Convert.ToDateTime(row["IssueDate"]),
                    ExpiryDate = Convert.ToDateTime(row["ExpiryDate"]),
                    Status = row["Status"].ToString(),
                    FilePath = row["FilePath"].ToString(),
                    EmployeeId = Convert.ToInt32(row["EmployeeID"]),
                    EmployeeName = row["EmployeeName"].ToString(),
                    EmployeeDepartment = row["EmployeeDepartment"]?.ToString()
                });
            }
            return certificates;
        }

        /// <summary>
        /// ???? ?????? AIS
        /// </summary>
        public List<CertificateViewModel> GetAISCertificates()
        {
            var certificates = new List<CertificateViewModel>();
            const string sql = @"
        SELECT 
            ce.CertificateId, 
            t.TypeName,
            ce.CertificateTitle AS Title,
            ce.IssueDate, ce.ExpiryDate, ce.Status, ce.FilePath,
            e.EmployeeID,
            e.FullName AS EmployeeName,
            e.Department AS EmployeeDepartment
        FROM 
            Certificates ce
        JOIN 
            Employees e ON ce.EmployeeId = e.EmployeeID
        LEFT JOIN
            DocumentTypes t ON ce.TypeId = t.TypeId
        WHERE
            ce.ControllerId IS NULL 
            AND e.Department LIKE '%AIS%';
    ";
            DataTable dt = ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                certificates.Add(new CertificateViewModel
                {
                    CertificateId = Convert.ToInt32(row["CertificateId"]),
                    TypeName = row["TypeName"].ToString(),
                    Title = row["Title"].ToString(),
                    IssueDate = Convert.ToDateTime(row["IssueDate"]),
                    ExpiryDate = Convert.ToDateTime(row["ExpiryDate"]),
                    Status = row["Status"].ToString(),
                    FilePath = row["FilePath"].ToString(),
                    EmployeeId = Convert.ToInt32(row["EmployeeID"]),
                    EmployeeName = row["EmployeeName"].ToString(),
                    EmployeeDepartment = row["EmployeeDepartment"]?.ToString()
                });
            }
            return certificates;
        }

        /// <summary>
        /// ???? ?????? CNS
        /// </summary>
        public List<CertificateViewModel> GetCNSCertificates()
        {
            var certificates = new List<CertificateViewModel>();
            const string sql = @"
        SELECT 
            ce.CertificateId, 
            t.TypeName,
            ce.CertificateTitle AS Title,
            ce.IssueDate, ce.ExpiryDate, ce.Status, ce.FilePath,
            e.EmployeeID,
            e.FullName AS EmployeeName,
            e.Department AS EmployeeDepartment
        FROM 
            Certificates ce
        JOIN 
            Employees e ON ce.EmployeeId = e.EmployeeID
        LEFT JOIN
            DocumentTypes t ON ce.TypeId = t.TypeId
        WHERE
            ce.ControllerId IS NULL 
            AND e.Department LIKE '%CNS%';
    ";
            DataTable dt = ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                certificates.Add(new CertificateViewModel
                {
                    CertificateId = Convert.ToInt32(row["CertificateId"]),
                    TypeName = row["TypeName"].ToString(),
                    Title = row["Title"].ToString(),
                    IssueDate = Convert.ToDateTime(row["IssueDate"]),
                    ExpiryDate = Convert.ToDateTime(row["ExpiryDate"]),
                    Status = row["Status"].ToString(),
                    FilePath = row["FilePath"].ToString(),
                    EmployeeId = Convert.ToInt32(row["EmployeeID"]),
                    EmployeeName = row["EmployeeName"].ToString(),
                    EmployeeDepartment = row["EmployeeDepartment"]?.ToString()
                });
            }
            return certificates;
        }

        /// <summary>
        /// ???? ?????? AFTN
        /// </summary>
        public List<CertificateViewModel> GetAFTNCertificates()
        {
            var certificates = new List<CertificateViewModel>();
            const string sql = @"
        SELECT 
            ce.CertificateId, 
            t.TypeName,
            ce.CertificateTitle AS Title,
            ce.IssueDate, ce.ExpiryDate, ce.Status, ce.FilePath,
            e.EmployeeID,
            e.FullName AS EmployeeName,
            e.Department AS EmployeeDepartment
        FROM 
            Certificates ce
        JOIN 
            Employees e ON ce.EmployeeId = e.EmployeeID
        LEFT JOIN
            DocumentTypes t ON ce.TypeId = t.TypeId
        WHERE
            ce.ControllerId IS NULL 
            AND e.Department LIKE '%AFTN%';
    ";
            DataTable dt = ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                certificates.Add(new CertificateViewModel
                {
                    CertificateId = Convert.ToInt32(row["CertificateId"]),
                    TypeName = row["TypeName"].ToString(),
                    Title = row["Title"].ToString(),
                    IssueDate = Convert.ToDateTime(row["IssueDate"]),
                    ExpiryDate = Convert.ToDateTime(row["ExpiryDate"]),
                    Status = row["Status"].ToString(),
                    FilePath = row["FilePath"].ToString(),
                    EmployeeId = Convert.ToInt32(row["EmployeeID"]),
                    EmployeeName = row["EmployeeName"].ToString(),
                    EmployeeDepartment = row["EmployeeDepartment"]?.ToString()
                });
            }
            return certificates;
        }

        /// <summary>
        /// ???? ?????? Ops Staff & Administration
        /// </summary>
        public List<CertificateViewModel> GetOpsStaffCertificates()
        {
            var certificates = new List<CertificateViewModel>();
            const string sql = @"
        SELECT 
            ce.CertificateId, 
            t.TypeName,
            ce.CertificateTitle AS Title,
            ce.IssueDate, ce.ExpiryDate, ce.Status, ce.FilePath,
            e.EmployeeID,
            e.FullName AS EmployeeName,
            e.Department AS EmployeeDepartment
        FROM 
            Certificates ce
        JOIN 
            Employees e ON ce.EmployeeId = e.EmployeeID
        LEFT JOIN
            DocumentTypes t ON ce.TypeId = t.TypeId
        WHERE
            ce.ControllerId IS NULL 
            AND (e.Department LIKE '%Administration%' OR e.Department LIKE '%Safety%' OR e.Department LIKE '%Quality%');
    ";
            DataTable dt = ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                certificates.Add(new CertificateViewModel
                {
                    CertificateId = Convert.ToInt32(row["CertificateId"]),
                    TypeName = row["TypeName"].ToString(),
                    Title = row["Title"].ToString(),
                    IssueDate = Convert.ToDateTime(row["IssueDate"]),
                    ExpiryDate = Convert.ToDateTime(row["ExpiryDate"]),
                    Status = row["Status"].ToString(),
                    FilePath = row["FilePath"].ToString(),
                    EmployeeId = Convert.ToInt32(row["EmployeeID"]),
                    EmployeeName = row["EmployeeName"].ToString(),
                    EmployeeDepartment = row["EmployeeDepartment"]?.ToString()
                });
            }
            return certificates;
        }

        public List<CertificateViewModel> GetAllCertificatesDetailsMix()
        {
            var allCertificates = new List<CertificateViewModel>();
            allCertificates.AddRange(GetControllerCertificates());
            allCertificates.AddRange(GetEmployeeCertificates());
            return allCertificates.OrderBy(l => l.PersonName).ToList();
            //return allCertificates.OrderBy(c => c.ControllerName ?? c.EmployeeName).ToList(); // ????? ??? ??? ?????
        }
        /// <summary>
        /// ???? ????? ????? ????? ??? ????? ????????
        /// </summary>
        public CertificateViewModel GetCertificateById(int certificateId)
        {
            // ??? ????????? ????? ?????? ???? ???? ?????? ?? ?????
            const string sql = @"
        SELECT 
            ce.CertificateId, ce.TypeId, ce.CertificateTitle, ce.Notes,
            ce.IssueDate, ce.ExpiryDate, ce.Status, ce.FilePath,
            ce.ControllerId, ce.EmployeeId,
            t.TypeName,
            c.FullName AS ControllerName,
            e.FullName AS EmployeeName
        FROM 
            Certificates ce
        LEFT JOIN 
            DocumentTypes t ON ce.TypeId = t.TypeId
        LEFT JOIN
            Controllers c ON ce.ControllerId = c.ControllerId
        LEFT JOIN
            Employees e ON ce.EmployeeId = e.EmployeeId
        WHERE 
            ce.CertificateId = @CertificateId;
    ";

            var parameters = new[] { new SqlParameter("@CertificateId", certificateId) };
            DataTable dt = ExecuteQuery(sql, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new CertificateViewModel
                {
                    CertificateId = Convert.ToInt32(row["CertificateId"]),
                    TypeId = row["TypeId"] == DBNull.Value ? 0 : Convert.ToInt32(row["TypeId"]),
                    TypeName = row["TypeName"]?.ToString(),
                    Title = row["CertificateTitle"]?.ToString(),
                    IssueDate = Convert.ToDateTime(row["IssueDate"]),
                    ExpiryDate = Convert.ToDateTime(row["ExpiryDate"]),
                    Status = row["Status"]?.ToString(),
                    FilePath = row["FilePath"]?.ToString(),
                    Notes = row["Notes"]?.ToString(),
                    ControllerId = row["ControllerId"] as int?,
                    EmployeeId = row["EmployeeId"] as int?,
                    // ??? ??????? ???? ????? ??????? ???? ???? ??? ?? ???? ?????
                    ControllerName = row["ControllerName"] as string,
                    EmployeeName = row["EmployeeName"] as string
                };
            }
            return null;
        }
        /// <summary>
        /// ???? ????? ?? ????? ????????
        /// </summary>
        public void DeleteCertificateById(int certificateId)
        {
            // Important: First get the file path to delete it from the server
            var certificate = GetCertificateById(certificateId);
            if (certificate != null && !string.IsNullOrEmpty(certificate.FilePath))
            {
                // Construct the physical path
                string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var physicalPath = Path.Combine(wwwRootPath, certificate.FilePath.TrimStart('/'));

                // Delete the file if it exists
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }

            // Now delete the record from the database
            const string sql = "DELETE FROM Certificates WHERE CertificateId = @CertificateId";
            ExecuteNonQuery(sql, new SqlParameter("@CertificateId", certificateId));
        }

        /// <summary>
        /// ???? ????? ?????? ???????? ??? Dropdown
        /// </summary>
        public List<SelectListItem> GetCertificateTypes()
        {
            // Make sure the table name 'DocumentTypes' is correct according to your database schema
            const string sql = "SELECT TypeId, TypeName FROM DocumentTypes ORDER BY TypeName";
            DataTable dt = ExecuteQuery(sql);
            return dt.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["TypeId"].ToString(),
                Text = row["TypeName"].ToString()
            }).ToList();
        }

        // --- ADD THESE TWO NEW METHODS TO YOUR SqlServerDb.cs FILE ---
        // --- ADD THESE TWO NEW METHODS TO YOUR SqlServerDb.cs FILE ---

        /// <summary>
        /// Fetches all observations related to Air Traffic Controllers.
        // --- ADD THESE TWO NEW METHODS TO YOUR SqlServerDb.cs FILE ---


        public List<Observation> GetControllerObservations()
        {
            var list = new List<Observation>();
            const string sql = @"
        SELECT o.*, c.FullName AS ControllerName, NULL AS EmployeeName
        FROM dbo.Observations o
        JOIN dbo.Controllers c ON o.ControllerId = c.ControllerId
        WHERE o.EmployeeId IS NULL OR o.EmployeeId = 0";
            DataTable dt = ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapDataRowToObservation(row));
            }
            return list;
        }

        public List<Observation> GetAISObservations()
        {
            var list = new List<Observation>();
            const string sql = @"
        SELECT o.*, e.FullName AS EmployeeName, NULL AS ControllerName
        FROM dbo.Observations o
        JOIN dbo.Employees e ON o.EmployeeId = e.EmployeeID
        WHERE (o.ControllerId IS NULL OR o.ControllerId = 0)
        AND e.Department LIKE '%AIS%'";
            DataTable dt = ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapDataRowToObservation(row));
            }
            return list;
        }

        public List<Observation> GetCNSObservations()
        {
            var list = new List<Observation>();
            const string sql = @"
        SELECT o.*, e.FullName AS EmployeeName, NULL AS ControllerName
        FROM dbo.Observations o
        JOIN dbo.Employees e ON o.EmployeeId = e.EmployeeID
        WHERE (o.ControllerId IS NULL OR o.ControllerId = 0)
        AND e.Department LIKE '%CNS%'";
            DataTable dt = ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapDataRowToObservation(row));
            }
            return list;
        }

        public List<Observation> GetAFTNObservations()
        {
            var list = new List<Observation>();
            const string sql = @"
        SELECT o.*, e.FullName AS EmployeeName, NULL AS ControllerName
        FROM dbo.Observations o
        JOIN dbo.Employees e ON o.EmployeeId = e.EmployeeID
        WHERE (o.ControllerId IS NULL OR o.ControllerId = 0)
        AND e.Department LIKE '%AFTN%'";
            DataTable dt = ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapDataRowToObservation(row));
            }
            return list;
        }

        public List<Observation> GetATFMObservations()
        {
            var list = new List<Observation>();
            const string sql = @"
        SELECT o.*, e.FullName AS EmployeeName, NULL AS ControllerName
        FROM dbo.Observations o
        JOIN dbo.Employees e ON o.EmployeeId = e.EmployeeID
        WHERE (o.ControllerId IS NULL OR o.ControllerId = 0)
        AND e.Department LIKE '%ATFM%'";
            DataTable dt = ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapDataRowToObservation(row));
            }
            return list;
        }

        public List<Observation> GetOpsStaffObservations()
        {
            var list = new List<Observation>();
            const string sql = @"
        SELECT o.*, e.FullName AS EmployeeName, NULL AS ControllerName
        FROM dbo.Observations o
        JOIN dbo.Employees e ON o.EmployeeId = e.EmployeeID
        WHERE (o.ControllerId IS NULL OR o.ControllerId = 0)
        AND (e.Department LIKE '%Administration%' OR e.Department LIKE '%Safety%' OR e.Department LIKE '%Quality%')";
            DataTable dt = ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapDataRowToObservation(row));
            }
            return list;
        }

        // Keep the old method for backward compatibility
        public List<Observation> GetEmployeeObservations()
        {
            var list = new List<Observation>();
            const string sql = @"
        SELECT o.*, e.FullName AS EmployeeName, NULL AS ControllerName
        FROM dbo.Observations o
        JOIN dbo.Employees e ON o.EmployeeId = e.EmployeeID
        WHERE o.ControllerId IS NULL OR o.ControllerId = 0";
            DataTable dt = ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapDataRowToObservation(row));
            }
            return list;
        }
        /// <summary>
        public void DeleteObservationById(int observationId)
        {
            const string sql = "DELETE FROM dbo.Observations WHERE ObservationId = @ObservationId";
            ExecuteNonQuery(sql, new SqlParameter("@ObservationId", observationId));
        }


        public List<Observation> GetAllObservationsDetails()
        {
            var allObservations = new List<Observation>();
            allObservations.AddRange(GetControllerObservations());
            allObservations.AddRange(GetEmployeeObservations());
            return allObservations.OrderBy(o => o.ControllerName ?? o.EmployeeName).ToList(); // ????? ??? ??? ?????
        }

        private Observation MapDataRowToObservation(DataRow row)
        {
            return new Observation
            {
                ObservationId = Convert.ToInt32(row["ObservationId"]),
                ObservationNo = row.Table.Columns.Contains("ObservationNo") && row["ObservationNo"] != DBNull.Value ? Convert.ToInt32(row["ObservationNo"]) : null,
                LicenseNumber = row.Table.Columns.Contains("LicenseNumber") ? row["LicenseNumber"].ToString() : null,
                Notes = row.Table.Columns.Contains("Notes") ? row["Notes"].ToString() : null,
                FilePath = row.Table.Columns.Contains("FilePath") ? row["FilePath"].ToString() : null,
                FlightNo = row.Table.Columns.Contains("FlightNo") ? row["FlightNo"].ToString() : null,
                DurationDays = row.Table.Columns.Contains("Duration_Days") && row["Duration_Days"] != DBNull.Value ? Convert.ToInt32(row["Duration_Days"]) : null,
                TravelCountry = row.Table.Columns.Contains("TravelCountry") ? row["TravelCountry"].ToString() : null,
                DepartDate = row.Table.Columns.Contains("DepartDate") && row["DepartDate"] != DBNull.Value ? Convert.ToDateTime(row["DepartDate"]) : null,
                ReturnDate = row.Table.Columns.Contains("ReturnDate") && row["ReturnDate"] != DBNull.Value ? Convert.ToDateTime(row["ReturnDate"]) : null,
                ControllerId = row.Table.Columns.Contains("ControllerId") && row["ControllerId"] != DBNull.Value ? Convert.ToInt32(row["ControllerId"]) : null,
                // The important part: check if the column exists before reading
                ControllerName = row.Table.Columns.Contains("ControllerName") && row["ControllerName"] != DBNull.Value ? row["ControllerName"].ToString() : null,
                EmployeeId = row.Table.Columns.Contains("EmployeeId") && row["EmployeeId"] != DBNull.Value ? Convert.ToInt32(row["EmployeeId"]) : null,
                EmployeeName = row.Table.Columns.Contains("EmployeeName") && row["EmployeeName"] != DBNull.Value ? row["EmployeeName"].ToString() : null
            };
        }

        public int GetNextObservationNumberForPerson(int? controllerId, int? employeeId)
        {
            string sql;
            SqlParameter parameter;
            if (controllerId.HasValue)
            {
                sql = "SELECT COUNT(*) FROM dbo.Observations WHERE ControllerId = @Id";
                parameter = new SqlParameter("@Id", controllerId.Value);
            }
            else if (employeeId.HasValue)
            {
                sql = "SELECT COUNT(*) FROM dbo.Observations WHERE EmployeeId = @Id";
                parameter = new SqlParameter("@Id", employeeId.Value);
            }
            else return 1;

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add(parameter);
                    connection.Open();
                    var result = command.ExecuteScalar();
                    return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) + 1 : 1;
                }
            }
        }



        /// <summary>
        /// Fetches a simple list of all divisions (airports) for dropdowns.
        /// </summary>
        public List<SelectListItem> GetAllDivisionsForSelectList()
        {
            const string sql = "SELECT AirportId, AirportName FROM dbo.Airports ORDER BY AirportName";
            DataTable dt = ExecuteQuery(sql);
            return dt.AsEnumerable().Select(row => new SelectListItem
            {
                Value = row["AirportId"].ToString(),
                Text = row["AirportName"].ToString()
            }).ToList();
        }

        // You likely already have methods to get all controllers and employees.
        // If not, you can add simple versions like these:

        /// <summary>
        /// Fetches a simple list of all controllers for dropdowns.
        /// </summary>
        public DataTable GetAllControllersForSelectList()
        {
            const string sql = "SELECT ControllerId, FullName FROM dbo.Controllers ORDER BY FullName";
            return ExecuteQuery(sql);
        }

        /// <summary>
        /// Fetches a simple list of all employees for dropdowns.
        /// </summary>
        public DataTable GetAllEmployeesForSelectList()
        {
            const string sql = "SELECT EmployeeID, FullName FROM dbo.Employees ORDER BY FullName";
            return ExecuteQuery(sql);
        }

        /// <summary>
        /// Inserts a new project into the database and returns its new ID.
        /// </summary>
        public int AddProject(Project project)
        {
            const string sql = @"
        INSERT INTO Projects (ProjectName, Description, Location, AssociatedEntity, Status, StartDate, EndDate, FolderPath)
        OUTPUT INSERTED.ProjectId
        VALUES (@ProjectName, @Description, @Location, @AssociatedEntity, @Status, @StartDate, @EndDate, @FolderPath);
    ";

            var parameters = new[]
            {
        new SqlParameter("@ProjectName", project.ProjectName),
        new SqlParameter("@Description", (object)project.Description ?? DBNull.Value),
        new SqlParameter("@Location", (object)project.Location ?? DBNull.Value),
        new SqlParameter("@AssociatedEntity", (object)project.AssociatedEntity ?? DBNull.Value),
        new SqlParameter("@Status", project.Status),
        new SqlParameter("@StartDate", (object)project.StartDate ?? DBNull.Value),
        new SqlParameter("@EndDate", (object)project.EndDate ?? DBNull.Value),
        new SqlParameter("@FolderPath", (object)project.FolderPath ?? DBNull.Value)
    };

            // ExecuteScalar is used here to get the first value returned, which is the new ProjectId.
            return (int)ExecuteScalar(sql, parameters);
        }

        /// <summary>
        /// Inserts a list of participants (controllers or employees) for a given project.
        /// </summary>
        public void AddProjectParticipants(int projectId, List<int> controllerIds, List<int> employeeIds)
        {
            // We create one single query to insert all participants in one go.
            var sql = "";
            var parameters = new List<SqlParameter>();
            int paramIndex = 0;

            foreach (var controllerId in controllerIds)
            {
                sql += $"INSERT INTO ProjectParticipants (ProjectId, ControllerId) VALUES (@ProjectId{paramIndex}, @ControllerId{paramIndex});\n";
                parameters.Add(new SqlParameter($"@ProjectId{paramIndex}", projectId));
                parameters.Add(new SqlParameter($"@ControllerId{paramIndex}", controllerId));
                paramIndex++;
            }

            foreach (var employeeId in employeeIds)
            {
                sql += $"INSERT INTO ProjectParticipants (ProjectId, EmployeeId) VALUES (@ProjectId{paramIndex}, @EmployeeId{paramIndex});\n";
                parameters.Add(new SqlParameter($"@ProjectId{paramIndex}", projectId));
                parameters.Add(new SqlParameter($"@EmployeeId{paramIndex}", employeeId));
                paramIndex++;
            }

            if (!string.IsNullOrEmpty(sql))
            {
                ExecuteNonQuery(sql, parameters.ToArray());
            }
        }

        /// <summary>
        /// Inserts a list of divisions for a given project.
        /// </summary>
        public void AddProjectDivisions(int projectId, List<int> divisionIds)
        {
            var sql = "";
            var parameters = new List<SqlParameter>();
            int paramIndex = 0;

            foreach (var divisionId in divisionIds)
            {
                sql += $"INSERT INTO ProjectDivisions (ProjectId, DivisionId) VALUES (@ProjectId{paramIndex}, @DivisionId{paramIndex});\n";
                parameters.Add(new SqlParameter($"@ProjectId{paramIndex}", projectId));
                parameters.Add(new SqlParameter($"@DivisionId{paramIndex}", divisionId));
                paramIndex++;
            }

            if (!string.IsNullOrEmpty(sql))
            {
                ExecuteNonQuery(sql, parameters.ToArray());
            }
        }

        public List<Project> GetAllProjects()
        {
            var list = new List<Project>();
            const string sql = "SELECT * FROM dbo.Projects ORDER BY StartDate DESC";
            DataTable dt = ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Project
                {
                    ProjectId = Convert.ToInt32(row["ProjectId"]),
                    ProjectName = row["ProjectName"].ToString(),
                    Description = row["Description"].ToString(),
                    Location = row["Location"].ToString(),
                    AssociatedEntity = row["AssociatedEntity"].ToString(),
                    Status = row["Status"].ToString(),
                    StartDate = row["StartDate"] as DateTime?,
                    EndDate = row["EndDate"] as DateTime?,
                    FolderPath = row["FolderPath"].ToString()
                });
            }
            return list;
        }

        /// <summary>
        /// Fetches all participants for all projects to be mapped in the code.
        /// Returns a lookup table for easy access.
        /// </summary>
        public ILookup<int, string> GetAllProjectParticipants()
        {
            var list = new List<Tuple<int, string>>();
            // This query combines names from both Controllers and Employees for all projects
            const string sql = @"
        SELECT 
            pp.ProjectId, 
            ISNULL(c.FullName, e.FullName) AS ParticipantName
        FROM 
            ProjectParticipants pp
        LEFT JOIN 
            Controllers c ON pp.ControllerId = c.ControllerId
        LEFT JOIN 
            Employees e ON pp.EmployeeId = e.EmployeeID";

            DataTable dt = ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Tuple<int, string>(Convert.ToInt32(row["ProjectId"]), row["ParticipantName"].ToString()));
            }
            // Group by ProjectId
            return list.ToLookup(t => t.Item1, t => t.Item2);
        }

        /// <summary>
        /// Fetches all divisions for all projects to be mapped in the code.
        /// </summary>
        public ILookup<int, string> GetAllProjectDivisions()
        {
            var list = new List<Tuple<int, string>>();
            const string sql = @"
        SELECT 
            pd.ProjectId,
            a.AirportName
        FROM 
            ProjectDivisions pd
        JOIN 
            Airports a ON pd.DivisionId = a.AirportId";

            DataTable dt = ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Tuple<int, string>(Convert.ToInt32(row["ProjectId"]), row["AirportName"].ToString()));
            }
            return list.ToLookup(t => t.Item1, t => t.Item2);
        }


        // Add these new methods inside your SqlServerDb.cs class

        /// <summary>
        /// Fetches a single project by its ID.
        /// </summary>
        // ??? ????? ????? ????? GetProjectById ??? ?? ??? ??????
        public Project GetProjectById(int projectId)
        {
            Project project = null;
            const string sql = "SELECT * FROM dbo.Projects WHERE ProjectId = @ProjectId";
            var dt = ExecuteQuery(sql, new SqlParameter("@ProjectId", projectId));

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                project = new Project
                {
                    ProjectId = Convert.ToInt32(row["ProjectId"]),
                    ProjectName = row["ProjectName"].ToString(),
                    Description = row["Description"].ToString(),
                    Location = row["Location"].ToString(),
                    AssociatedEntity = row["AssociatedEntity"].ToString(),
                    Status = row["Status"].ToString(),
                    StartDate = row["StartDate"] as DateTime?,
                    EndDate = row["EndDate"] as DateTime?,
                    FolderPath = row["FolderPath"]?.ToString()
                };
            }
            return project;
        }

        /// <summary>
        /// Fetches all participants (controllers and employees) for a specific project.
        /// </summary>
        public List<ProjectParticipantViewModel> GetParticipantsByProjectId(int projectId)
        {
            var list = new List<ProjectParticipantViewModel>();
            const string sql = @"
        SELECT 
            ISNULL(c.FullName, e.FullName) AS ParticipantName,
            CASE 
                WHEN c.ControllerId IS NOT NULL THEN 'Controller'
                WHEN e.EmployeeID IS NOT NULL THEN 'Employee'
                ELSE 'Unknown'
            END AS ParticipantType
        FROM 
            ProjectParticipants pp
        LEFT JOIN 
            Controllers c ON pp.ControllerId = c.ControllerId
        LEFT JOIN 
            Employees e ON pp.EmployeeId = e.EmployeeID
        WHERE 
            pp.ProjectId = @ProjectId
        ORDER BY
            ParticipantName;
    ";

            var dt = ExecuteQuery(sql, new SqlParameter("@ProjectId", projectId));

            foreach (DataRow row in dt.Rows)
            {
                var name = row["ParticipantName"].ToString();
                var type = row["ParticipantType"].ToString();

                list.Add(new ProjectParticipantViewModel
                {
                    Name = name,
                    Type = type,
                    AvatarText = name.Contains(" ") ? string.Concat(name.Split(' ').Select(s => s[0])) : (name.Length > 1 ? name.Substring(0, 2) : name).ToUpper(),
                    AvatarCssClass = type == "Controller" ? "avatar-controller" : "avatar-employee"
                });
            }
            return list;
        }

        /// <summary>
        /// Fetches all division names for a specific project.
        /// </summary>
        public List<string> GetDivisionsByProjectId(int projectId)
        {
            var list = new List<string>();
            const string sql = @"
        SELECT 
            a.AirportName
        FROM 
            ProjectDivisions pd
        JOIN 
            Airports a ON pd.DivisionId = a.AirportId
        WHERE
            pd.ProjectId = @ProjectId
        ORDER BY
            a.AirportName;
    ";
            DataTable dt = ExecuteQuery(sql, new SqlParameter("@ProjectId", projectId));
            foreach (DataRow row in dt.Rows)
            {
                list.Add(row["AirportName"].ToString());
            }
            return list;
        }

        // Add these new methods to your SqlServerDb.cs class

        /// <summary>
        /// Updates an existing project's main details in the database.
        /// </summary>
        public void UpdateProject(Project project)
        {
            const string sql = @"
        UPDATE Projects SET
            ProjectName = @ProjectName,
            Description = @Description,
            Location = @Location,
            AssociatedEntity = @AssociatedEntity,
            Status = @Status,
            StartDate = @StartDate,
            EndDate = @EndDate
        WHERE ProjectId = @ProjectId;
    ";

            var parameters = new[]
            {
        new SqlParameter("@ProjectName", project.ProjectName),
        new SqlParameter("@Description", (object)project.Description ?? DBNull.Value),
        new SqlParameter("@Location", (object)project.Location ?? DBNull.Value),
        new SqlParameter("@AssociatedEntity", (object)project.AssociatedEntity ?? DBNull.Value),
        new SqlParameter("@Status", project.Status),
        new SqlParameter("@StartDate", (object)project.StartDate ?? DBNull.Value),
        new SqlParameter("@EndDate", (object)project.EndDate ?? DBNull.Value),
        new SqlParameter("@ProjectId", project.ProjectId)
    };

            ExecuteNonQuery(sql, parameters);
        }

        /// <summary>
        /// Replaces the participants for a given project. It first deletes all
        /// existing participants and then adds the new list.
        /// </summary>
        public void UpdateProjectParticipants(int projectId, List<int> controllerIds, List<int> employeeIds)
        {
            // Start a transaction to ensure both operations succeed or fail together
            using var conn = GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                // 1. Delete all existing participants for this project
                var deleteSql = "DELETE FROM ProjectParticipants WHERE ProjectId = @ProjectId";
                using (var cmdDelete = new SqlCommand(deleteSql, conn, tx))
                {
                    cmdDelete.Parameters.AddWithValue("@ProjectId", projectId);
                    cmdDelete.ExecuteNonQuery();
                }

                // 2. Add the new participants (using the existing AddProjectParticipants method logic)
                if ((controllerIds != null && controllerIds.Any()) || (employeeIds != null && employeeIds.Any()))
                {
                    var addSql = "";
                    var parameters = new List<SqlParameter>();
                    int paramIndex = 0;

                    foreach (var controllerId in controllerIds ?? new List<int>())
                    {
                        addSql += $"INSERT INTO ProjectParticipants (ProjectId, ControllerId) VALUES (@pId{paramIndex}, @cId{paramIndex});\n";
                        parameters.Add(new SqlParameter($"@pId{paramIndex}", projectId));
                        parameters.Add(new SqlParameter($"@cId{paramIndex}", controllerId));
                        paramIndex++;
                    }

                    foreach (var employeeId in employeeIds ?? new List<int>())
                    {
                        addSql += $"INSERT INTO ProjectParticipants (ProjectId, EmployeeId) VALUES (@pId{paramIndex}, @eId{paramIndex});\n";
                        parameters.Add(new SqlParameter($"@pId{paramIndex}", projectId));
                        parameters.Add(new SqlParameter($"@eId{paramIndex}", employeeId));
                        paramIndex++;
                    }

                    if (!string.IsNullOrEmpty(addSql))
                    {
                        using (var cmdAdd = new SqlCommand(addSql, conn, tx))
                        {
                            cmdAdd.Parameters.AddRange(parameters.ToArray());
                            cmdAdd.ExecuteNonQuery();
                        }
                    }
                }

                tx.Commit();
            }
            catch (Exception)
            {
                tx.Rollback();
                throw; // Re-throw the exception to be handled by the controller
            }
        }


        /// <summary>
        /// Replaces the divisions for a given project.
        /// </summary>
        public void UpdateProjectDivisions(int projectId, List<int> divisionIds)
        {
            using var conn = GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                // 1. Delete old divisions
                var deleteSql = "DELETE FROM ProjectDivisions WHERE ProjectId = @ProjectId";
                using (var cmdDelete = new SqlCommand(deleteSql, conn, tx))
                {
                    cmdDelete.Parameters.AddWithValue("@ProjectId", projectId);
                    cmdDelete.ExecuteNonQuery();
                }

                // 2. Add new divisions
                if (divisionIds != null && divisionIds.Any())
                {
                    var addSql = "";
                    var parameters = new List<SqlParameter>();
                    int paramIndex = 0;
                    foreach (var divisionId in divisionIds)
                    {
                        addSql += $"INSERT INTO ProjectDivisions (ProjectId, DivisionId) VALUES (@pId{paramIndex}, @dId{paramIndex});\n";
                        parameters.Add(new SqlParameter($"@pId{paramIndex}", projectId));
                        parameters.Add(new SqlParameter($"@dId{paramIndex}", divisionId));
                        paramIndex++;
                    }
                    using (var cmdAdd = new SqlCommand(addSql, conn, tx))
                    {
                        cmdAdd.Parameters.AddRange(parameters.ToArray());
                        cmdAdd.ExecuteNonQuery();
                    }
                }

                tx.Commit();
            }
            catch (Exception)
            {
                tx.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Gets the IDs of the selected participants for the edit page.
        /// </summary>
        public List<string> GetSelectedParticipantIds(int projectId)
        {
            var list = new List<string>();
            const string sql = @"
        SELECT 
            'c-' + CAST(ControllerId AS VARCHAR) AS ParticipantId
        FROM ProjectParticipants WHERE ProjectId = @ProjectId AND ControllerId IS NOT NULL
        UNION ALL
        SELECT 
            'e-' + CAST(EmployeeId AS VARCHAR) AS ParticipantId
        FROM ProjectParticipants WHERE ProjectId = @ProjectId AND EmployeeId IS NOT NULL;
    ";
            DataTable dt = ExecuteQuery(sql, new SqlParameter("@ProjectId", projectId));
            foreach (DataRow row in dt.Rows)
            {
                list.Add(row["ParticipantId"].ToString());
            }
            return list;
        }

        /// <summary>
        /// Gets the IDs of the selected divisions for the edit page.
        /// </summary>
        public List<int> GetSelectedDivisionIds(int projectId)
        {
            var list = new List<int>();
            const string sql = "SELECT DivisionId FROM ProjectDivisions WHERE ProjectId = @ProjectId;";
            DataTable dt = ExecuteQuery(sql, new SqlParameter("@ProjectId", projectId));
            foreach (DataRow row in dt.Rows)
            {
                list.Add(Convert.ToInt32(row["DivisionId"]));
            }
            return list;
        }

        /// <param name="projectId">The ID of the project to delete.</param>
        public void DeleteProject(int projectId)
        {
            const string sql = "DELETE FROM Projects WHERE ProjectId = @ProjectId";
            ExecuteNonQuery(sql, new SqlParameter("@ProjectId", projectId));
        }
        public Employee GetEmployeeByUsername(string username)
        {
            string sql = @"
                SELECT e.*, u.Username 
                FROM Employees e
                JOIN Users u ON e.UserID = u.UserID
                WHERE u.Username = @Username";
            var dt = ExecuteQuery(sql, new SqlParameter("@Username", username));
            return dt.Rows.Count > 0 ? MapDataRowToEmployee(dt.Rows[0]) : null;
        }

        /// </summary>
        public List<License> GetLicensesByEmployeeId(int employeeId)
        {
            const string sql = "SELECT * FROM Licenses WHERE EmployeeID = @EmployeeID ORDER BY ExpiryDate ASC";
            var dt = ExecuteQuery(sql, new SqlParameter("@EmployeeID", employeeId));
            var list = new List<License>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new License
                {
                    LicenseId = Convert.ToInt32(row["licenseid"]),
                    TypeName = row["licensetype"].ToString(),
                    // FIX: Use the safe 'as' operator for nullable types
                    ExpiryDate = row["expirydate"] as DateTime?,
                    IssueDate = row["IssueDate"] as DateTime?,
                    FilePath = row["pdfpath"]?.ToString(),
                });
            }
            return list;
        }

        public List<CertificateViewModel> GetCertificatesByEmployeeId(int employeeId)
        {
            const string sql = @"
                SELECT cer.*, t.typename 
                FROM certificates cer
                JOIN documenttypes t ON cer.typeid = t.typeid
                WHERE cer.EmployeeId = @EmployeeId ORDER BY cer.certificateid ASC";
            var dt = ExecuteQuery(sql, new SqlParameter("@EmployeeId", employeeId));
            var list = new List<CertificateViewModel>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new CertificateViewModel
                {
                    CertificateId = Convert.ToInt32(row["certificateid"]),
                    TypeName = row["typename"].ToString(),
                    Title = row["certificatetitle"].ToString(),
                    // FIX: Use the safe 'as' operator for nullable types
                    IssueDate = row["IssueDate"] != DBNull.Value ? Convert.ToDateTime(row["IssueDate"]) : default,
                    ExpiryDate = row["expirydate"] != DBNull.Value ? Convert.ToDateTime(row["expirydate"]) : default,
                    Status = row["status"].ToString(),
                    FilePath = row["filepath"]?.ToString(),
                });
            }
            return list;
        }

        public List<Observation> GetObservationsByEmployeeId(int employeeId)
        {
            const string sql = "SELECT * FROM observations WHERE EmployeeId = @EmployeeId ORDER BY departdate ASC";
            var dt = ExecuteQuery(sql, new SqlParameter("@EmployeeId", employeeId));
            var list = new List<Observation>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Observation
                {
                    ObservationId = Convert.ToInt32(row["observationid"]),
                    TravelCountry = row["travelcountry"]?.ToString(),
                    LicenseNumber = row["licensenumber"]?.ToString(),
                    DurationDays = row["duration_days"] as int?,
                    // FIX: Use the safe 'as' operator for nullable types
                    DepartDate = row["departdate"] as DateTime?,
                    ReturnDate = row["returndate"] as DateTime?,
                    Notes = row["notes"]?.ToString(),
                });
            }
            return list;
        }

        public List<UserProjectViewModel> GetProjectsByParticipant(int? controllerId, int? employeeId)
        {
            var list = new List<UserProjectViewModel>();
            const string sql = @"
                SELECT p.ProjectId, p.ProjectName, p.Status, p.StartDate, p.EndDate,Description 
                FROM Projects p JOIN ProjectParticipants pp ON p.ProjectId = pp.ProjectId
                WHERE (@controllerId IS NOT NULL AND pp.ControllerId = @controllerId)
                   OR (@employeeId IS NOT NULL AND pp.EmployeeId = @employeeId)
                ORDER BY p.StartDate DESC;";
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@controllerId", (object)controllerId ?? DBNull.Value),
                new SqlParameter("@employeeId", (object)employeeId ?? DBNull.Value)
            };
            DataTable dt = ExecuteQuery(sql, parameters.ToArray());
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new UserProjectViewModel
                {
                    ProjectId = Convert.ToInt32(row["ProjectId"]),
                    ProjectName = row["ProjectName"].ToString(),
                    Status = row["Status"].ToString(),
                    StartDate = row["StartDate"] as DateTime?,
                    EndDate = row["EndDate"] as DateTime?,
                    Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : null

                });
            }
            return list;
        }
        // ?????? ??? ?????? ??????? ?? ??? SqlServerDb.cs

        public ProfileViewModel GetProfileDataByUsername(string username)
        {
            var vm = new ProfileViewModel();
            int? controllerId = null;
            int? employeeId = null;

            var controllerData = GetControllerByUsername(username);
            if (controllerData != null)
            {
                controllerId = Convert.ToInt32(controllerData["controllerid"]);

                // Populate common and controller-specific fields
                vm.UserType = "Controller";
                vm.UserId = controllerId.Value;
                vm.FullName = controllerData["fullname"].ToString();
                vm.Username = controllerData["username"].ToString();
                vm.JobTitle = controllerData["job_title"]?.ToString();
                vm.PhotoPath = controllerData["photopath"]?.ToString();
                vm.Email = controllerData["email"]?.ToString();
                vm.PhoneNumber = controllerData["Phone_Number"]?.ToString();
                vm.DateOfBirth = controllerData["DATE_OF_BIRTH"] as DateTime?;
                vm.MaritalStatus = controllerData["MARITAL_STATUS"]?.ToString();
                vm.Address = controllerData["address"]?.ToString();
                vm.HireDate = controllerData["HIRE_DATE"] as DateTime?;
                vm.CurrentDepartment = controllerData["CURRENT_DEPARTMENT"]?.ToString();
                vm.EmploymentStatus = controllerData["Employment_Status"]?.ToString();
                vm.EducationLevel = controllerData["Education_Level"]?.ToString();
                vm.EmergencyContact = controllerData["Emergency_Contact"]?.ToString();

                vm.Licenses = GetLicensesByController(username);
                vm.Certificates = GetCertificatesByController(username);
                vm.Observations = GetObservationsByController(username);
            }
            else
            {
                var employeeData = GetEmployeeByUsername(username);
                if (employeeData != null)
                {
                    employeeId = employeeData.EmployeeID;

                    // Populate common and employee-specific fields
                    vm.UserType = "Employee";
                    vm.UserId = employeeId.Value;
                    vm.FullName = employeeData.FullName;
                    vm.Username = employeeData.Username;
                    vm.JobTitle = employeeData.JobTitle;
                    vm.Email = employeeData.Email;
                    vm.PhoneNumber = employeeData.PhoneNumber;
                    vm.HireDate = employeeData.HireDate;
                    vm.CurrentDepartment = employeeData.Department;
                    vm.Address = employeeData.Address;
                    vm.EmploymentStatus = employeeData.IsActive ? "Active" : "Inactive";
                    vm.EmergencyContact = employeeData.EmergencyContactPhone;
                    vm.Gender = employeeData.Gender;
                    vm.Location = employeeData.Location;

                    vm.Licenses = GetLicensesByEmployeeId(employeeId.Value);
                    vm.Certificates = GetCertificatesByEmployeeId(employeeId.Value);
                    vm.Observations = GetObservationsByEmployeeId(employeeId.Value);
                }
                else { return null; }
            }
            vm.Projects = GetDetailedProjectsForParticipant(controllerId, employeeId);
            return vm;
        }

        // ... (The rest of your SqlServerDb.cs methods like UpdateController, Project methods, etc.) ...
        private Employee MapDataRowToEmployee(DataRow row)
        {
            return new Employee
            {
                EmployeeID = Convert.ToInt32(row["EmployeeID"]),
                EmployeeOfficialID = row["EmployeeOfficialID"] != DBNull.Value ? row["EmployeeOfficialID"].ToString() : string.Empty,
                UserID = row["UserID"] != DBNull.Value ? Convert.ToInt32(row["UserID"]) : (int?)null,
                FullName = row["FullName"] != DBNull.Value ? row["FullName"].ToString() : string.Empty,
                JobTitle = row["JobTitle"] != DBNull.Value ? row["JobTitle"].ToString() : string.Empty,
                Department = row["Department"] != DBNull.Value ? row["Department"].ToString() : string.Empty,
                PhoneNumber = row["PhoneNumber"] != DBNull.Value ? row["PhoneNumber"].ToString() : string.Empty,
                Email = row["Email"] != DBNull.Value ? row["Email"].ToString() : string.Empty,
                HireDate = row["HireDate"] != DBNull.Value ? Convert.ToDateTime(row["HireDate"]) : (DateTime?)null,
                TerminationDate = row["TerminationDate"] != DBNull.Value ? Convert.ToDateTime(row["TerminationDate"]) : (DateTime?)null,
                IsActive = row["IsActive"] != DBNull.Value ? Convert.ToBoolean(row["IsActive"]) : false,
                Address = row["Address"] != DBNull.Value ? row["Address"].ToString() : string.Empty,
                Location = row["Location"] != DBNull.Value ? row["Location"].ToString() : string.Empty,
                EmergencyContactPhone = row["EmergencyContactPhone"] != DBNull.Value ? row["EmergencyContactPhone"].ToString() : string.Empty,
                Gender = row["Gender"] != DBNull.Value ? row["Gender"].ToString() : string.Empty,
                Username = row.Table.Columns.Contains("Username") && row["Username"] != DBNull.Value ? row["Username"].ToString() : string.Empty,
                //*************************
                // Additional Personal Information
                DateOfBirth = row.Table.Columns.Contains("DateOfBirth") && row["DateOfBirth"] != DBNull.Value ? Convert.ToDateTime(row["DateOfBirth"]) : (DateTime?)null,
                MaritalStatus = row.Table.Columns.Contains("MaritalStatus") && row["MaritalStatus"] != DBNull.Value ? row["MaritalStatus"].ToString() : string.Empty,
                EducationLevel = row.Table.Columns.Contains("EducationLevel") && row["EducationLevel"] != DBNull.Value ? row["EducationLevel"].ToString() : string.Empty,
                //*************************
                // Financial Information
                CurrentSalary = row.Table.Columns.Contains("CurrentSalary") && row["CurrentSalary"] != DBNull.Value ? Convert.ToDecimal(row["CurrentSalary"]) : (decimal?)null,
                AnnualIncreasePercentage = row.Table.Columns.Contains("AnnualIncreasePercentage") && row["AnnualIncreasePercentage"] != DBNull.Value ? Convert.ToDecimal(row["AnnualIncreasePercentage"]) : (decimal?)null,
                SalaryAfterAnnualIncrease = row.Table.Columns.Contains("SalaryAfterAnnualIncrease") && row["SalaryAfterAnnualIncrease"] != DBNull.Value ? Convert.ToDecimal(row["SalaryAfterAnnualIncrease"]) : (decimal?)null,
                BankAccountNumber = row.Table.Columns.Contains("BankAccountNumber") && row["BankAccountNumber"] != DBNull.Value ? row["BankAccountNumber"].ToString() : string.Empty,
                BankName = row.Table.Columns.Contains("BankName") && row["BankName"] != DBNull.Value ? row["BankName"].ToString() : string.Empty,
                TaxId = row.Table.Columns.Contains("TaxId") && row["TaxId"] != DBNull.Value ? row["TaxId"].ToString() : string.Empty,
                InsuranceNumber = row.Table.Columns.Contains("InsuranceNumber") && row["InsuranceNumber"] != DBNull.Value ? row["InsuranceNumber"].ToString() : string.Empty,
                PhotoPath = row.Table.Columns.Contains("PhotoPath") && row["PhotoPath"] != DBNull.Value ? row["PhotoPath"].ToString() : string.Empty,
                NeedLicense = row.Table.Columns.Contains("NeedLicense") && row["NeedLicense"] != DBNull.Value ? Convert.ToBoolean(row["NeedLicense"]) : true,
                OrganizationalStructure = row.Table.Columns.Contains("OrganizationalStructure") && row["OrganizationalStructure"] != DBNull.Value ? row["OrganizationalStructure"].ToString() : string.Empty,
                Division = row.Table.Columns.Contains("Division") && row["Division"] != DBNull.Value ? row["Division"].ToString() : string.Empty
            };
        }

        /// </summary>
        public List<ProfileProjectViewModel> GetDetailedProjectsForParticipant(int? controllerId, int? employeeId)
        {
            var projectsList = new List<ProfileProjectViewModel>();

            // 1. ??? ?? ???????? ???????? ????????
            var baseProjects = GetProjectsByParticipant(controllerId, employeeId);
            if (!baseProjects.Any())
            {
                return projectsList; // ????? ????? ????? ??? ?? ??? ???? ??????
            }

            var projectIds = baseProjects.Select(p => p.ProjectId).ToList();

            // 2. ??? ?? ????????? ??? ??????? ????? ??? ???????? ?? ????????? ???
            var allParticipants = GetAllProjectParticipants(); // ???? ?????? ????
            var allDivisions = GetAllProjectDivisions(); // ???? ?????? ????

            // 3. ???? ??????? ???????? ???????
            foreach (var baseProject in baseProjects)
            {
                // ????? ???? ?????? ??????? ???????
                var fullProject = GetProjectById(baseProject.ProjectId);
                if (fullProject != null)
                {
                    var detailedProject = new ProfileProjectViewModel
                    {
                        Project = fullProject,
                        Participants = allParticipants.Contains(baseProject.ProjectId) ? allParticipants[baseProject.ProjectId].ToList() : new List<string>(),
                        Divisions = allDivisions.Contains(baseProject.ProjectId) ? allDivisions[baseProject.ProjectId].ToList() : new List<string>()
                    };
                    projectsList.Add(detailedProject);
                }
            }

            return projectsList;
        }

        // Add these new methods inside your SqlServerDb.cs class

        /// <summary>
        /// Gets the count of projects for each status.
        /// </summary>
        //public Dictionary<string, int> GetProjectStatusCounts()
        //{
        //    const string sql = "SELECT Status, COUNT(*) AS Count FROM Projects GROUP BY Status";
        //    var dt = ExecuteQuery(sql);

        //    return dt.AsEnumerable()
        //             .ToDictionary(
        //                 row => row.Field<string>("Status"),
        //                 row => row.Field<int>("Count")
        //             );
        //}

        /// <summary>
        /// Gets the most recent projects added to the system.
        /// </summary>
        public List<Project> GetRecentProjects(int count = 5)
        {
            var list = new List<Project>();
            const string sql = "SELECT TOP (@count) * FROM Projects ORDER BY ProjectId DESC;";
            var dt = ExecuteQuery(sql, new SqlParameter("@count", count));
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Project
                {
                    ProjectId = Convert.ToInt32(row["ProjectId"]),
                    ProjectName = row["ProjectName"].ToString(),
                    Status = row["Status"].ToString(),
                    StartDate = row["StartDate"] as DateTime?
                });
            }
            return list;
        }

        /// <param name="newPassword">The new plaintext password to be hashed and stored.</param>
        public void UpdateUserPassword(string username, string newPassword)
        {
            // Use the correct ASP.NET Core Identity hasher to create a valid hash.
            var hashedPassword = _passwordHasher.HashPassword(null, newPassword);

            using var conn = GetConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                // 1. Update the central Users table with the new hash.
                const string sqlUsers = "UPDATE Users SET PasswordHash = @PasswordHash WHERE Username = @Username";
                using (var cmdUsers = new SqlCommand(sqlUsers, conn, tx))
                {
                    cmdUsers.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    cmdUsers.Parameters.AddWithValue("@Username", username);
                    cmdUsers.ExecuteNonQuery();
                }

                // 2. Also update the Controllers table to keep passwords in sync.
                const string sqlControllers = "UPDATE Controllers SET Password = @Password WHERE Username = @Username";
                using (var cmdControllers = new SqlCommand(sqlControllers, conn, tx))
                {
                    // This assumes the 'Password' column in the Controllers table is large enough (e.g., nvarchar(255))
                    // to store the full hash generated by ASP.NET Core Identity.
                    cmdControllers.Parameters.AddWithValue("@Password", hashedPassword);
                    cmdControllers.Parameters.AddWithValue("@Username", username);
                    cmdControllers.ExecuteNonQuery();
                }

                tx.Commit(); // Commit both changes if successful
            }
            catch (Exception)
            {
                tx.Rollback(); // Undo changes if anything fails
                throw;
            }
        }

        public void UpdateControllerProfile(ControllerUser user)
        {
            // This query now uses the correct column names from your database schema
            const string sql = @"
        UPDATE Controllers SET
            FullName = @FullName, 
            Email = @Email, 
            Phone_Number = @PhoneNumber,         -- Corrected column name
            Date_Of_Birth = @DateOfBirth,         -- Corrected column name
            Marital_Status = @MaritalStatus,       -- Corrected column name
            Address = @Address, 
            Emergency_Contact = @EmergencyContact, -- Corrected column name
            Education_Level = @EducationLevel      -- Corrected column name
        WHERE ControllerId = @ControllerId";

            ExecuteNonQuery(sql,
                new SqlParameter("@FullName", user.FullName),
                new SqlParameter("@Email", user.Email),
                new SqlParameter("@PhoneNumber", (object)user.PhoneNumber ?? DBNull.Value),
                new SqlParameter("@DateOfBirth", (object)user.DateOfBirth ?? DBNull.Value),
                new SqlParameter("@MaritalStatus", (object)user.MaritalStatus ?? DBNull.Value),
                new SqlParameter("@Address", (object)user.Address ?? DBNull.Value),
                new SqlParameter("@EmergencyContact", (object)user.EmergencyContact ?? DBNull.Value),
                new SqlParameter("@EducationLevel", (object)user.EducationLevel ?? DBNull.Value),
                new SqlParameter("@ControllerId", user.ControllerId)
            );
        }

        public void UpdateEmployeeProfile(Employee employee)
        {
            const string sql = @"
        UPDATE Employees SET
            FullName = @FullName, Email = @Email, PhoneNumber = @PhoneNumber,
            Address = @Address, EmergencyContactPhone = @EmergencyContactPhone,
            Gender = @Gender, Location = @Location
        WHERE EmployeeID = @EmployeeID";

            ExecuteNonQuery(sql,
                new SqlParameter("@FullName", employee.FullName),
                new SqlParameter("@Email", employee.Email),
                new SqlParameter("@PhoneNumber", employee.PhoneNumber),
                new SqlParameter("@Address", employee.Address),
                new SqlParameter("@EmergencyContactPhone", employee.EmergencyContactPhone),
                new SqlParameter("@Gender", employee.Gender),
                new SqlParameter("@Location", employee.Location),
                new SqlParameter("@EmployeeID", employee.EmployeeID)
            );
        }

        // Add these new methods inside your SqlServerDb.cs class

        // --- Division & Personnel Summaries ---

        /// <summary>
        /// Gets the count of personnel (Controllers + Employees) for each division (Airport).
        /// </summary>
        public List<ChartData> GetPersonnelCountByDivision()
        {
            const string sql = @"
        SELECT 
            a.AirportName AS Label, 
            COUNT(c.ControllerId) AS Value
        FROM Airports a
        LEFT JOIN Controllers c ON a.AirportId = c.AirportId
        GROUP BY a.AirportName
        ORDER BY Value DESC;
    ";
            var dt = ExecuteQuery(sql);
            var list = new List<ChartData>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ChartData
                {
                    Label = row["Label"].ToString(),
                    Value = Convert.ToInt32(row["Value"])
                });
            }
            return list;
        }

        public int GetTotalPersonnelCount()
        {
            const string sql = "SELECT (SELECT COUNT(*) FROM Controllers) + (SELECT COUNT(*) FROM Employees)";
            object result = ExecuteScalar(sql);
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        // --- License & Certificate Summaries ---

        public Dictionary<string, int> GetLicenseStatusCounts()
        {
            var counts = new Dictionary<string, int>
    {
        { "Expired", 0 },
        { "ExpiringSoon", 0 },
        { "Valid", 0 }
    };

            // Expired licenses (up to today)
            counts["Expired"] = Convert.ToInt32(ExecuteScalar("SELECT COUNT(*) FROM Licenses WHERE ExpiryDate < GETDATE()"));

            // Licenses expiring in the next 30 days
            counts["ExpiringSoon"] = Convert.ToInt32(ExecuteScalar("SELECT COUNT(*) FROM Licenses WHERE ExpiryDate BETWEEN GETDATE() AND DATEADD(day, 30, GETDATE())"));

            // Valid licenses (expiring after 30 days)
            counts["Valid"] = Convert.ToInt32(ExecuteScalar("SELECT COUNT(*) FROM Licenses WHERE ExpiryDate > DATEADD(day, 30, GETDATE())"));

            return counts;
        }

        public List<ChartData> GetTopStaffByCertificateCount(int topN = 5)
        {
            const string sql = @"
        SELECT TOP (@topN)
            ISNULL(c.FullName, e.FullName) AS Label,
            COUNT(cert.CertificateId) AS Value
        FROM Certificates cert
        LEFT JOIN Controllers c ON cert.ControllerId = c.ControllerId
        LEFT JOIN Employees e ON cert.EmployeeId = e.EmployeeID
        GROUP BY ISNULL(c.FullName, e.FullName)
        HAVING ISNULL(c.FullName, e.FullName) IS NOT NULL
        ORDER BY Value DESC;
    ";
            var dt = ExecuteQuery(sql, new SqlParameter("@topN", topN));
            var list = new List<ChartData>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ChartData
                {
                    Label = row["Label"].ToString(),
                    Value = Convert.ToInt32(row["Value"])
                });
            }
            return list;
        }


        // --- Observation & Project Summaries ---

        public int GetTotalObservationDays()
        {
            object result = ExecuteScalar("SELECT SUM(Duration_Days) FROM Observations");
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        public List<ChartData> GetProjectCountByUser(int topN = 5)
        {
            const string sql = @"
        SELECT TOP (@topN)
            ParticipantName AS Label,
            COUNT(ProjectId) AS Value
        FROM (
            SELECT p.ProjectId, c.FullName AS ParticipantName FROM Projects p JOIN ProjectParticipants pp ON p.ProjectId = pp.ProjectId JOIN Controllers c ON pp.ControllerId = c.ControllerId
            UNION ALL
            SELECT p.ProjectId, e.FullName AS ParticipantName FROM Projects p JOIN ProjectParticipants pp ON p.ProjectId = pp.ProjectId JOIN Employees e ON pp.EmployeeId = e.EmployeeID
        ) AS ProjectParticipants
        GROUP BY ParticipantName
        ORDER BY Value DESC;
    ";
            var dt = ExecuteQuery(sql, new SqlParameter("@topN", topN));
            var list = new List<ChartData>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ChartData
                {
                    Label = row["Label"].ToString(),
                    Value = Convert.ToInt32(row["Value"])
                });
            }
            return list;
        }

        // --- Existing Project Methods (ensure they are present) ---

        public Dictionary<string, int> GetProjectStatusCounts()
        {
            const string sql = "SELECT Status, COUNT(*) AS Count FROM Projects GROUP BY Status";
            var dt = ExecuteQuery(sql);
            return dt.AsEnumerable().ToDictionary(row => row.Field<string>("Status"), row => row.Field<int>("Count"));
        }


        public int GetTotalEmployeesCount()
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM Employees");
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        /// <summary>
        /// Gets details of active projects (e.g., projects with 'In Progress' status).
        /// </summary>
        public List<Project> GetActiveProjectsDetails()
        {
            var projects = new List<Project>();
            // ????? ????? ???? ??????? ??? ??? ?????? ???????? "??????"
            const string sql = "SELECT * FROM Projects WHERE Status = 'In Progress' ORDER BY StartDate DESC";
            var dt = ExecuteQuery(sql);

            foreach (DataRow row in dt.Rows)
            {
                projects.Add(new Project
                {
                    ProjectId = Convert.ToInt32(row["ProjectId"]),
                    ProjectName = row["ProjectName"].ToString(),
                    Description = row["Description"]?.ToString(), // ???? ?? ??????? ?? ????? Nullable
                    Location = row["Location"]?.ToString(),
                    AssociatedEntity = row["AssociatedEntity"]?.ToString(),
                    Status = row["Status"].ToString(),
                    StartDate = row["StartDate"] as DateTime?,
                    EndDate = row["EndDate"] as DateTime?,
                    FolderPath = row["FolderPath"]?.ToString()
                });
            }
            return projects;
        }

        public List<ProjectFile> GetFilesByProjectId(int projectId)
        {
            var files = new List<ProjectFile>();

            // 1. ??? ???? ?????? ????? ???????? ?? ????? ????????
            var projectInfo = ExecuteQuery("SELECT FolderPath FROM Projects WHERE ProjectId = @ProjectId", new[] { new SqlParameter("@ProjectId", projectId) });
            if (projectInfo.Rows.Count == 0 || projectInfo.Rows[0]["FolderPath"] == DBNull.Value)
            {
                return files; // ?? ???? ???? ??????? ???? ????? ?????
            }
            string relativeFolderPath = projectInfo.Rows[0]["FolderPath"].ToString();

            // 2. ????? ?????? ?????? (?????? ?? ?????? ???) ??? ???? ???? ??? ???????
            // wwwroot ?? ?????? ????? ?? ?????? ASP.NET Core
            string physicalFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativeFolderPath.TrimStart('/'));

            // 3. ?????? ?? ???? ?????? ??? ?????? ????? ???????
            if (!Directory.Exists(physicalFolderPath))
            {
                return files; // ?????? ??? ?????? ???? ????? ?????
            }

            // 4. ????? ???? ??????? ???????? ?? ??????
            var filePaths = Directory.GetFiles(physicalFolderPath);

            foreach (var filePath in filePaths)
            {
                var fileInfo = new FileInfo(filePath);
                files.Add(new ProjectFile
                {
                    Name = fileInfo.Name,
                    Size = $"{Math.Round(fileInfo.Length / 1024.0, 1)} KB", // ???? ????? ???????????
                    Url = Path.Combine("/", relativeFolderPath, fileInfo.Name).Replace('\\', '/') // ????? ???? ????? ????
                });
            }

            return files;
        }

        public List<Project> GetProjectsByEmployeeId(int employeeId)
        {
            var list = new List<Project>();
            const string sql = @"
        SELECT p.*
        FROM Projects p
        JOIN ProjectParticipants pp ON p.ProjectId = pp.ProjectId
        WHERE pp.EmployeeId = @EmployeeId
        ORDER BY p.StartDate DESC;
    ";

            var dt = ExecuteQuery(sql, new SqlParameter("@EmployeeId", employeeId));

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Project
                {
                    ProjectId = Convert.ToInt32(row["ProjectId"]),
                    ProjectName = row["ProjectName"].ToString(),
                    Description = row["Description"]?.ToString(),
                    Location = row["Location"]?.ToString(),
                    AssociatedEntity = row["AssociatedEntity"]?.ToString(),
                    Status = row["Status"].ToString(),
                    StartDate = row["StartDate"] as DateTime?,
                    EndDate = row["EndDate"] as DateTime?,
                    FolderPath = row["FolderPath"]?.ToString()
                });
            }
            return list;
        }
        public List<Employee> GetAllEmployees()
        {
            var list = new List<Employee>();
            // Assuming you have a helper method 'MapDataRowToEmployee'
            const string sql = "SELECT * FROM Employees ORDER BY FullName";
            var dt = ExecuteQuery(sql);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapDataRowToEmployee(row));
            }
            return list;
        }
        public EmployeeDetailViewModel GetEmployeeDetailsById(int employeeId)
        {
            var employee = GetSingleEmployeeById(employeeId);
            if (employee == null)
            {
                return null;
            }

            var viewModel = new EmployeeDetailViewModel
            {
                Employee = employee,
                Licenses = GetLicensesByEmployeeId(employeeId),
                Certificates = GetCertificatesByEmployeeId(employeeId),
                Observations = GetObservationsByEmployeeId(employeeId),
                Projects = GetDetailedProjectsForParticipant(null, employeeId)
            };

            return viewModel;
        }

        /// <summary>
        /// Helper method to get a single Employee object from the database.
        /// You might already have this or a similar method.
        /// </summary>
        public Employee GetSingleEmployeeById(int employeeId)
        {
            const string sql = @"
                SELECT e.*, u.Username 
                FROM Employees e
                LEFT JOIN Users u ON e.UserID = u.UserID
                WHERE e.EmployeeID = @EmployeeID";
            var dt = ExecuteQuery(sql, new SqlParameter("@EmployeeID", employeeId));

            if (dt.Rows.Count > 0)
            {
                return MapDataRowToEmployee(dt.Rows[0]);
            }
            return null;
        }



    } // End of class SqlServerDb
} // End of namespace


























