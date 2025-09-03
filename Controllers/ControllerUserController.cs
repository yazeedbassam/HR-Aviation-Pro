using DocuSign.eSign.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SendGrid.Helpers.Mail;
using System;
//using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data; // Required for DBNull.Value
using System.Data.SqlClient; // ???? ?? ??????? ??? ??? namespace
using System.Drawing; // ??????? ?????? ??? ????
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WebApplication1.DataAccess;
using WebApplication1.DataAccess; // ???? ?? ??? ?????? SqlDb ???
using WebApplication1.DataAccess; // Assume _db is of type SqlDb
using WebApplication1.Models;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.Attributes;
using Color = System.Drawing.Color; // Assume 'model' is of a Controller-related type


[Authorize]
public class ControllerUserController : Controller
{
    private readonly SqlServerDb _db;
    private readonly IPasswordHasher<ControllerUser> _hasher; // ???? ControllerUserViewModel
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IAdvancedPermissionManagerService _permissionService;

    public ControllerUserController(
        SqlServerDb db,
        IPasswordHasher<ControllerUser> hasher, // ???? ControllerUserViewModel
        IHostEnvironment hostEnvironment,
        IAdvancedPermissionManagerService permissionService)
    {
        _db = db;
        _hasher = hasher;
        _hostEnvironment = hostEnvironment; // ???? ?? ??? ????? ?????
        _permissionService = permissionService;
    }
    // GET: /ControllerUser
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        if (!await _permissionService.CanPerformOperationAsync(userId, "Controller", "View"))
        {
            return Forbid();
        }
        // The SQL query now includes a JOIN with the Countries table
        const string sql = @"
        SELECT 
            c.controllerid, c.fullname, c.username, c.airportid,
            a.airportname, a.icao_code AS airporticao,
            co.CountryName,  -- The new column
            c.licensepath, c.photopath, c.job_title, c.education_level,
            c.date_of_birth, c.marital_status, c.phone_number, c.email,
            c.address, c.hire_date, c.employment_status, c.current_department,
            c.transfer_date, c.emergency_contact, c.LicenseNumber
        FROM 
            controllers c
        LEFT JOIN 
            airports a ON c.airportid = a.airportid
        LEFT JOIN
            Countries co ON a.CountryId = co.CountryId"; // The new JOIN

        var dt = _db.ExecuteQuery(sql);

        var list = dt.Rows.Cast<DataRow>()
            .Select(r => new ControllerUser
            {
                ControllerId = Convert.ToInt32(r["controllerid"]),
                FullName = r["fullname"].ToString(),
                Username = r["username"].ToString(),
                AirportId = r["airportid"] != DBNull.Value ? Convert.ToInt32(r["airportid"]) : 0,
                AirportName = r["airportname"].ToString(),
                icao_code = r["airporticao"].ToString(),
                CountryName = r["CountryName"].ToString(), // Mapping the new property
                LicensePath = r["licensepath"].ToString(),
                PhotoPath = r["photopath"].ToString(),
                JobTitle = r["job_title"].ToString(),
                EducationLevel = r["education_level"].ToString(),
                DateOfBirth = r["date_of_birth"] as DateTime?,
                MaritalStatus = r["marital_status"].ToString(),
                PhoneNumber = r["phone_number"].ToString(),
                Email = r["email"].ToString(),
                Address = r["address"].ToString(),
                HireDate = r["hire_date"] as DateTime?,
                EmploymentStatus = r["employment_status"].ToString(),
                CurrentDepartment = r["current_department"].ToString(),
                TransferDate = r["transfer_date"] as DateTime?,
                EmergencyContact = r["emergency_contact"].ToString(),
                LicenseNumber = r["LicenseNumber"].ToString(),
            })
            .ToList();

        return View(list);
    }


    // GET: /ControllerUser/Create
    [HttpGet]
    public IActionResult Create()
    {
        LoadCountriesAndAirports();
        LoadRoles();
        LoadJobTitle();
        LoadEducationLevel();
        LoadEmploymentStatusLevel();
        LoadMaritalStatusLevel();
        LoadCurrentDepartment();
        return View(new ControllerUser());
    }

    // POST: /ControllerUser/Create
    // POST: /ControllerUser/Create
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(ControllerUser model)
    {
        LoadCountriesAndAirports();
        LoadRoles();
        LoadJobTitle();
        LoadEducationLevel();
        LoadEmploymentStatusLevel();
        LoadMaritalStatusLevel();
        LoadCurrentDepartment();

        if (!ModelState.IsValid)
            return View(model);

        // 1) ??? ????? ??????
        if (_db.GetUserByUsername(model.Username) != null)
        {
            ModelState.AddModelError("Username", "This username is already taken.");
            return View(model);
        }

        // 2) Validate that the role exists in Roles table before creating user
        if (string.IsNullOrEmpty(model.Role))
        {
            ModelState.AddModelError("Role", "Role is required.");
            return View(model);
        }
        
        // Check if role exists in Roles table
        var roleExists = _db.ExecuteQuery("SELECT COUNT(*) FROM Roles WHERE RoleName = @RoleName", 
            new Microsoft.Data.SqlClient.SqlParameter("@RoleName", model.Role));
        
        if (Convert.ToInt32(roleExists.Rows[0][0]) == 0)
        {
            ModelState.AddModelError("Role", $"Role '{model.Role}' does not exist in the system.");
            return View(model);
        }
        
        // Create user with validated role
        _db.CreateUser(model.Username, model.Password, model.Role);
        var userRec = _db.GetUserByUsername(model.Username);
        int userId = userRec?.userId ?? 0;

        // 2.1) Ensure new Controller users only get minimal permissions (Profile access only)
        if (model.Role == "Controller" && userId > 0)
        {
            // Remove any default role-based permissions that might have been assigned
            // This ensures Controller users only get explicitly assigned permissions
            _db.RemoveDefaultRolePermissionsForUser(userId, "Controller");
        }

        // 3) ??? ???????
        string sanitized = SanitizeFolderName(model.Username);
        string photoPath = SaveUploadedFile(model.PhotoFile, "uploads", sanitized, "default.jpg");
        string licensePath = SaveUploadedFile(model.LicenseFile, "licenses", sanitized, "default.pdf");

        // 4) INSERT ?? ?????? ??????? ? sequence
        const string sql = @"
INSERT INTO controllers (
    fullname,
    username,
    password,
    airportid,
    photopath,
    licensepath,
    userid,
    job_title,
    education_level,
    date_of_birth,
    marital_status,
    phone_number,
    email,
    address,
    hire_date,
    employment_status,
    current_department,
    transfer_date,
    emergency_contact,
    LicenseNumber,
    NeedLicense,
    IsActive,
    CurrentSalary,
    AnnualIncreasePercentage,
    SalaryAfterAnnualIncrease,
    BankAccountNumber,
    BankName,
    TaxId,
    InsuranceNumber
) VALUES (
    @fullName,
    @userName,
    @password,
    @airportId,
    @photoPath,
    @licensePath,
    @userId,
    @jobTitle,
    @educationLevel,
    @dateOfBirth,
    @maritalStatus,
    @phoneNumber,
    @email,
    @address,
    @hireDate,
    @employmentStatus,
    @currentDepartment,
    @transferDate,
    @emergencyContact,
    @LicenseNumber,
    @NeedLicense,
    @IsActive,
    @CurrentSalary,
    @AnnualIncreasePercentage,
    @SalaryAfterAnnualIncrease,
    @BankAccountNumber,
    @BankName,
    @TaxId,
    @InsuranceNumber
)"; // <== ?? ??????? ???

        var parameters = new[]
       {
    new Microsoft.Data.SqlClient.SqlParameter("@fullName",           model.FullName),
    new Microsoft.Data.SqlClient.SqlParameter("@userName",            model.Username),
    new Microsoft.Data.SqlClient.SqlParameter("@password",            model.Password), // ??? ????? ???? ??? ???? ??????? ???? ???? ?????? ?????
    new Microsoft.Data.SqlClient.SqlParameter("@airportId",           model.AirportId),
    new Microsoft.Data.SqlClient.SqlParameter("@photoPath",           (object?)photoPath            ?? DBNull.Value),
    new Microsoft.Data.SqlClient.SqlParameter("@licensePath",         (object?)licensePath          ?? DBNull.Value),
    new Microsoft.Data.SqlClient.SqlParameter("@userId",              userId), // ???? ?? userId ?? ?????? ???? ????? ??????
    new Microsoft.Data.SqlClient.SqlParameter("@jobTitle",            (object?)model.JobTitle       ?? DBNull.Value),
    new Microsoft.Data.SqlClient.SqlParameter("@educationLevel",      (object?)model.EducationLevel ?? DBNull.Value),
    new Microsoft.Data.SqlClient.SqlParameter("@dateOfBirth",         (object?)model.DateOfBirth    ?? DBNull.Value),
    new Microsoft.Data.SqlClient.SqlParameter("@maritalStatus",       (object?)model.MaritalStatus  ?? DBNull.Value),
    new Microsoft.Data.SqlClient.SqlParameter("@phoneNumber",         (object?)model.PhoneNumber    ?? DBNull.Value),
    new Microsoft.Data.SqlClient.SqlParameter("@email",               (object?)model.Email          ?? DBNull.Value),
    new Microsoft.Data.SqlClient.SqlParameter("@address",             (object?)model.Address        ?? DBNull.Value),
    new Microsoft.Data.SqlClient.SqlParameter("@hireDate",            (object?)model.HireDate       ?? DBNull.Value),
    new Microsoft.Data.SqlClient.SqlParameter("@employmentStatus",    (object?)model.EmploymentStatus ?? DBNull.Value),
    new Microsoft.Data.SqlClient.SqlParameter("@currentDepartment",   (object?)model.CurrentDepartment?? DBNull.Value),
    new Microsoft.Data.SqlClient.SqlParameter("@transferDate",        (object?)model.TransferDate   ?? DBNull.Value),
         new Microsoft.Data.SqlClient.SqlParameter("@emergencyContact",    (object?)model.EmergencyContact ?? DBNull.Value),
     new Microsoft.Data.SqlClient.SqlParameter("@LicenseNumber",    (object?)model.LicenseNumber ?? DBNull.Value),
     new Microsoft.Data.SqlClient.SqlParameter("@NeedLicense",    model.NeedLicense),
     new Microsoft.Data.SqlClient.SqlParameter("@IsActive",    model.IsActive),
     new Microsoft.Data.SqlClient.SqlParameter("@CurrentSalary",    (object?)model.CurrentSalary ?? DBNull.Value),
     new Microsoft.Data.SqlClient.SqlParameter("@AnnualIncreasePercentage",    (object?)model.AnnualIncreasePercentage ?? DBNull.Value),
     new Microsoft.Data.SqlClient.SqlParameter("@SalaryAfterAnnualIncrease",    (object?)model.SalaryAfterAnnualIncrease ?? DBNull.Value),
     new Microsoft.Data.SqlClient.SqlParameter("@BankAccountNumber",    (object?)model.BankAccountNumber ?? DBNull.Value),
     new Microsoft.Data.SqlClient.SqlParameter("@BankName",    (object?)model.BankName ?? DBNull.Value),
     new Microsoft.Data.SqlClient.SqlParameter("@TaxId",    (object?)model.TaxId ?? DBNull.Value),
     new Microsoft.Data.SqlClient.SqlParameter("@InsuranceNumber",    (object?)model.InsuranceNumber ?? DBNull.Value)
};

        int rows = _db.ExecuteNonQuery(sql, parameters);
        if (rows > 0)
        {
            TempData["SuccessMessage"] = "Controller created successfully!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Failed to save controller.");
        return View(model);
    }


    /// <summary>
    /// *============ HELPER METHODS ============

    private void LoadRoles()
    {
        // Get roles directly from Roles table to ensure consistency
        try
        {
            var rolesQuery = "SELECT RoleName, Description FROM Roles WHERE RoleName IS NOT NULL ORDER BY RoleName";
            var rolesData = _db.ExecuteQuery(rolesQuery);
            
            ViewBag.Roles = rolesData.Rows.Cast<DataRow>()
                .Select(r => new SelectListItem(
                    r["Description"]?.ToString() ?? r["RoleName"].ToString(),  // Display Text: Description if available, otherwise RoleName
                    r["RoleName"].ToString()   // Value: RoleName for database operations
                ))
                .ToList();
        }
        catch
        {
            // Fallback to hardcoded values if database query fails
            ViewBag.Roles = new List<SelectListItem>
            {
                new SelectListItem("مدير النظام", "Admin"),
                new SelectListItem("مراقب", "Controller"),
                new SelectListItem("موظف", "Employee"),
                new SelectListItem("مشرف OJAI", "SuperVisor OJAI"),
                new SelectListItem("مشرف OJAM", "SuperVisor OJAM"),
                new SelectListItem("مشرف OJAQ", "SuperVisor OJAQ"),
                new SelectListItem("مشرف TACC", "SuperVisor TACC")
            };
        }
    }

    private void LoadJobTitle()
    {
        // Use Configuration Service instead of hardcoded values
        var configService = HttpContext.RequestServices.GetService<IConfigurationService>();
        if (configService != null)
        {
            ViewBag.JobTitle = configService.GetDropdownValues("JobTitles");
        }
        else
        {
            // Fallback to hardcoded values if service is not available
            ViewBag.JobTitle = new List<SelectListItem>
             {
                    new SelectListItem("ATC OJTI","ATC OJTI"),
                    new SelectListItem("ATC","ATC"),
                    new SelectListItem("Training Supervisor OJAI","Training Supervisor OJAI"),
                    new SelectListItem("Training Supervisor  OJAM","Training Supervisor  OJAM"),
                    new SelectListItem("Training Supervisor TACC","Training Supervisor TACC"),
                    new SelectListItem("Head of Department OJAI-Tower","Head of Department OJAI-Tower"),
                    new SelectListItem("Head of Department OJAM-Tower","Head of Department OJAM-Tower"),
                    new SelectListItem("Head of Department-TACC","Head of Department-TACC"),
                    new SelectListItem("OJAI DANS","OJAI DANS"),
                    new SelectListItem("OJAM DANS","OJAM DANS"),
                    new SelectListItem("OJAQ DANS","OJAQ DANS"),
                    new SelectListItem("CARC DANS","CARC DANS"),
                     new SelectListItem("Others","Others"),
                };
        }
    }

    private void LoadEducationLevel()
    {
        // Use Configuration Service instead of hardcoded values
        var configService = HttpContext.RequestServices.GetService<IConfigurationService>();
        if (configService != null)
        {
            ViewBag.EducationLevel = configService.GetConfigurationSelectListWithDefault("EducationLevels");
        }
        else
        {
            // Fallback to hardcoded values if service is not available
            ViewBag.EducationLevel = new List<SelectListItem> {
                new SelectListItem("Diploma", "Diploma"),
                new SelectListItem("Bachelor", "Bachelor"),
                new SelectListItem("Master", "Master"),
                new SelectListItem("Dr", "Dr"),
                new SelectListItem("Other", "Other")
            };
        }
    }
    private void LoadEmploymentStatusLevel()
    {
        // Use Configuration Service instead of hardcoded values
        var configService = HttpContext.RequestServices.GetService<IConfigurationService>();
        if (configService != null)
        {
            ViewBag.EmploymentStatus = configService.GetConfigurationSelectListWithDefault("EmploymentStatus");
        }
        else
        {
            // Fallback to hardcoded values if service is not available
            ViewBag.EmploymentStatus = new List<SelectListItem> {
                new SelectListItem("OJTI", "OJTI"),
                new SelectListItem("ATC", "ATC"),
                new SelectListItem("SuperVisor", "SuperVisor"),
                new SelectListItem("Instructor", "Instructor"),
                new SelectListItem("Examiner", "Examiner"),
                new SelectListItem("Manager", "Manager"),
                new SelectListItem("Others", "Others")
            };
        }
    }
    private void LoadMaritalStatusLevel()
    {
        // Use Configuration Service instead of hardcoded values
        var configService = HttpContext.RequestServices.GetService<IConfigurationService>();
        if (configService != null)
        {
            ViewBag.MaritalStatus = configService.GetConfigurationSelectListWithDefault("MaritalStatus");
        }
        else
        {
            // Fallback to hardcoded values if service is not available
            ViewBag.MaritalStatus = new List<SelectListItem> {
                new SelectListItem("Single", "Single"),
                new SelectListItem("Married", "Married"),
                new SelectListItem("Other", "Other")
            };
        }
    }
    private void LoadCurrentDepartment()
    {
        // Use Configuration Service instead of hardcoded values
        var configService = HttpContext.RequestServices.GetService<IConfigurationService>();
        if (configService != null)
        {
            ViewBag.CurrentDepartment = configService.GetConfigurationSelectListWithDefault("Departments");
        }
        else
        {
            // Fallback to hardcoded values if service is not available
            ViewBag.CurrentDepartment = new List<SelectListItem> {
                new SelectListItem("Queen", "Queen "),
                new SelectListItem("Aqaba", "Aqaba"),
                new SelectListItem("Amman ", "Amman"),
                new SelectListItem("TACC", "TACC"),
                new SelectListItem("CARC", "CARC")
            };
        }
    }

    private void LoadCountriesAndAirports()
    {
        // Countries
        var dtC = _db.ExecuteQuery("SELECT countryid,countryname FROM countries ORDER BY countryname");
        ViewBag.Countries = dtC.Rows.Cast<DataRow>()
            .Select(r => new SelectListItem
            {
                Value = r["countryid"].ToString(),
                Text = r["countryname"].ToString()
            }).ToList();

        // Airports (with ICAO code)
        var dtA = _db.ExecuteQuery(
            "SELECT airportid,airportname,icao_code FROM airports ORDER BY airportname");
        ViewBag.Airports = dtA.Rows.Cast<DataRow>()
            .Select(r => new SelectListItem
            {
                Value = r["airportid"].ToString(),
                Text = $"{r["airportname"]} ({r["icao_code"]})"
            }).ToList();
    }

    private string SaveUploadedFile(IFormFile file, string folderCategory, string userFolder, string defaultFileName)
    {
        if (file == null || file.Length == 0)
            return $"/{folderCategory}/default_user_{defaultFileName}";

        // ???? ???? ??????
        string root = Directory.GetCurrentDirectory();
        string target = Path.Combine(root, "wwwroot", folderCategory, userFolder);
        Directory.CreateDirectory(target);

        // ??? ????
        string unique = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        string full = Path.Combine(target, unique);
        using var fs = new FileStream(full, FileMode.Create);
        file.CopyTo(fs);

        return $"/{folderCategory}/{userFolder}/{unique}";
    }

    private string SanitizeFolderName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "default_user";
        string invalid = new string(Path.GetInvalidFileNameChars())
                       + new string(Path.GetInvalidPathChars());
        return Regex.Replace(name, $"[{Regex.Escape(invalid)}]", "_");
    }

    private List<SelectListItem> GetCountrySelectList()
    {
        var dt = _db.ExecuteQuery("SELECT countryid, countryname FROM countries");
        return dt.Rows.Cast<DataRow>()
                 .Select(r => new SelectListItem
                 {
                     Value = r["countryid"].ToString(),
                     Text = r["countryname"].ToString()
                 })
                 .ToList();
    }

    private DataTable GetAirportsFromDb()
    {
        // ??? ???? ????? ??????? ?????? ??? ???????? ??????? ?? ????? ???????
        // ??? ?? ???? ??? DataTable ????? ??? AirportId, AirportName, CountryId
        string airportQuery = "SELECT AirportId, AirportName,icao_code, CountryId FROM Airports"; // ???? ?? ????? ??????? ?????
        return _db.ExecuteQuery(airportQuery); // ????? ?? _db ?? ???? ?????? ?????? ????????
    }

    private void PopulateAirportsViewBag()
    {
        DataTable dtAirports = GetAirportsFromDb();
        var airportsList = new List<object>();
        foreach (DataRow row in dtAirports.Rows)
        {
            airportsList.Add(new
            {
                AirportId = Convert.ToInt32(row["AirportId"]),
                AirportName = row["AirportName"].ToString(),
                icao_code = row["icao_code"].ToString(), // ?? ????? icao_code ???
                CountryId = Convert.ToInt32(row["CountryId"])
            });
        }
        ViewBag.Airports = airportsList;
    }

    private string ComputeSha256Hash(string raw)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        byte[] bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(raw));
        return Convert.ToBase64String(bytes);
    }



    /// </summary>

    //*********************************************
    // GET: /ControllerUser/Edit
    [HttpGet]
    public IActionResult Edit(int id)
    {
        // 1) ??? ???????? ?? ????? ?? ???? USERS
        const string sql = @"
    SELECT c.controllerid,
            c.fullname,
            c.username,
            u.passwordhash AS password,
            c.airportid,
            a.CountryId, -- <<== ??? ?? ????? ?????? ???? ??????
            u.rolename AS role,
            c.job_title,
            c.education_level,
            c.date_of_birth,
            c.marital_status,
            c.phone_number,
            c.email,
            c.address,
            c.hire_date,
            c.employment_status,
            c.current_department,
            c.transfer_date,
            c.emergency_contact,
            c.LicenseNumber,
            c.NeedLicense,
            c.IsActive,
            c.CurrentSalary,
            c.AnnualIncreasePercentage,
            c.SalaryAfterAnnualIncrease,
            c.BankAccountNumber,
            c.BankName,
            c.TaxId,
            c.InsuranceNumber
            FROM controllers c
            JOIN users u ON c.userid = u.userid
            LEFT JOIN airports a ON c.airportid = a.airportid -- <<== ???? ?? ????? ??????
        WHERE c.controllerid = @ControllerId";
        var dt = _db.ExecuteQuery(sql, new Microsoft.Data.SqlClient.SqlParameter("@ControllerId", id)); // <== ?? ???????


        if (dt.Rows.Count == 0)
            return NotFound();

        var row = dt.Rows[0];
        var model = new ControllerUser
        {
            ControllerId = Convert.ToInt32(row["controllerid"]),
            FullName = row["fullname"].ToString()!,
            Username = row["username"].ToString()!,
            Password = row["password"].ToString()!,   // ??? ?? ????? ??????
            Role = row["role"].ToString()!,
            AirportId = Convert.ToInt32(row["airportid"]),

            // PhotoPath = row["photopath"].ToString(),
            //LicensePath = row["licensepath"].ToString(),

            JobTitle = row["job_title"].ToString(),
            EducationLevel = row["education_level"].ToString(),
            DateOfBirth = row["date_of_birth"] as DateTime?,
            MaritalStatus = row["marital_status"].ToString(),
            PhoneNumber = row["phone_number"].ToString(),
            Email = row["email"].ToString(),
            Address = row["address"].ToString(),
            HireDate = row["hire_date"] as DateTime?,
            EmploymentStatus = row["employment_status"].ToString(),
            CurrentDepartment = row["current_department"].ToString(),
            TransferDate = row["transfer_date"] as DateTime?,
            EmergencyContact = row["emergency_contact"].ToString(),
            LicenseNumber = row["LicenseNumber"].ToString(),
            NeedLicense = Convert.ToBoolean(row["NeedLicense"]),
            IsActive = Convert.ToBoolean(row["IsActive"]),
            CurrentSalary = row["CurrentSalary"] != DBNull.Value ? Convert.ToDecimal(row["CurrentSalary"]) : null,
            AnnualIncreasePercentage = row["AnnualIncreasePercentage"] != DBNull.Value ? Convert.ToDecimal(row["AnnualIncreasePercentage"]) : null,
            SalaryAfterAnnualIncrease = row["SalaryAfterAnnualIncrease"] != DBNull.Value ? Convert.ToDecimal(row["SalaryAfterAnnualIncrease"]) : null,
            BankAccountNumber = row["BankAccountNumber"]?.ToString(),
            BankName = row["BankName"]?.ToString(),
            TaxId = row["TaxId"]?.ToString(),
            InsuranceNumber = row["InsuranceNumber"]?.ToString()
        };
        ViewBag.SelectedCountryId = row["CountryId"] != DBNull.Value ? Convert.ToInt32(row["CountryId"]) : 0;

        LoadCountriesAndAirports();
        LoadRoles();
        LoadJobTitle();
        LoadEducationLevel();
        LoadEmploymentStatusLevel();
        LoadMaritalStatusLevel();
        LoadCurrentDepartment();
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(ControllerUser model)
    {
        // Remove file validation errors since files are optional
        ModelState.Remove("PhotoFile");
        ModelState.Remove("LicenseFile");
        
        LoadCountriesAndAirports();
        LoadRoles();
        LoadJobTitle();
        LoadEducationLevel();
        LoadEmploymentStatusLevel();
        LoadMaritalStatusLevel();
        LoadCurrentDepartment();

        if (!ModelState.IsValid)
            return View(model);

        // ???? ???????? ??? ?????? ??? ?? ???? ???? ?? ?????
        string newHashedPassword = string.IsNullOrEmpty(model.Password)
            ? _db.GetUserPasswordHashByUserId(model.UserId)
            : _hasher.HashPassword(model, model.Password);

        // ??? ????? ????? ?? ????
        string sanitizedUsername = SanitizeFolderName(model.Username);
        string photoPath = model.PhotoPath;
        string licensePath = model.LicensePath;

        // Handle photo upload
        if (model.PhotoFile != null && model.PhotoFile.Length > 0)
        {
            photoPath = SaveUploadedFile(model.PhotoFile, "uploads", sanitizedUsername, "default.jpg");
            Console.WriteLine($"Photo uploaded for controller {model.Username}: {photoPath}");
        }
        
        // Handle license upload
        if (model.LicenseFile != null && model.LicenseFile.Length > 0)
        {
            licensePath = SaveUploadedFile(model.LicenseFile, "licenses", sanitizedUsername, "default.pdf");
            Console.WriteLine($"License uploaded for controller {model.Username}: {licensePath}");
        }

        // ?????? ????????? ????? ????? ???????? ?? ????????
        using (var conn = _db.GetConnection())
        {
            conn.Open();
            using (var tx = conn.BeginTransaction())
            {
                // 1) ????? users ??????
                const string updUser = @"
                UPDATE users SET passwordhash=@pwdHash, rolename=@role WHERE username=@username";
                                        using (var cmdUser = new SqlCommand(updUser, conn, tx))
                {
                    cmdUser.Parameters.AddWithValue("@pwdHash", newHashedPassword);
                    cmdUser.Parameters.AddWithValue("@role", model.Role ?? (object)DBNull.Value);
                    cmdUser.Parameters.AddWithValue("@username", model.Username);
                    cmdUser.ExecuteNonQuery();
                }

                // 2) ????? controllers ??????
                const string updCtrl = @"
                UPDATE controllers SET
                    fullname = @fullName,
                    airportid = @airportId,
                    photopath = @photoPath,
                    licensepath = @licensePath,
                    job_title = @jobTitle,
                    education_level = @educationLevel,
                    date_of_birth = @dateOfBirth,
                    marital_status = @maritalStatus,
                    phone_number = @phoneNumber,
                    email = @email,
                    address = @address,
                    hire_date = @hireDate,
                    employment_status = @employmentStatus,
                    current_department = @currentDepartment,
                    transfer_date = @transferDate,
                    emergency_contact = @emergencyContact,
                    LicenseNumber =@LicenseNumber,
                    NeedLicense = @NeedLicense,
                    IsActive = @IsActive,
                    CurrentSalary = @CurrentSalary,
                    AnnualIncreasePercentage = @AnnualIncreasePercentage,
                    SalaryAfterAnnualIncrease = @SalaryAfterAnnualIncrease,
                    BankAccountNumber = @BankAccountNumber,
                    BankName = @BankName,
                    TaxId = @TaxId,
                    InsuranceNumber = @InsuranceNumber,
                    password = @pwdHash
                WHERE controllerid = @ControllerId";
                                        using (var cmdCtrl = new SqlCommand(updCtrl, conn, tx))
                {
                    cmdCtrl.Parameters.AddWithValue("@ControllerId", model.ControllerId);
                    cmdCtrl.Parameters.AddWithValue("@pwdHash", newHashedPassword);
                    //cmdCtrl.Parameters.AddWithValue("@role", model.Role ?? (object)DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@fullName", (object?)model.FullName ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@airportId", (object?)model.AirportId ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@photoPath", (object?)photoPath ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@licensePath", (object?)licensePath ?? DBNull.Value);
                    
                    // Log the photo and license paths being saved
                    Console.WriteLine($"Saving photo path: {photoPath}, license path: {licensePath} for controller {model.Username}");
                    
                    cmdCtrl.Parameters.AddWithValue("@jobTitle", (object?)model.JobTitle ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@educationLevel", (object?)model.EducationLevel ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@dateOfBirth", (object?)model.DateOfBirth ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@maritalStatus", (object?)model.MaritalStatus ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@phoneNumber", (object?)model.PhoneNumber ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@email", (object?)model.Email ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@address", (object?)model.Address ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@hireDate", (object?)model.HireDate ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@employmentStatus", (object?)model.EmploymentStatus ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@currentDepartment", (object?)model.CurrentDepartment ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@transferDate", (object?)model.TransferDate ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@emergencyContact", (object?)model.EmergencyContact ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@LicenseNumber", (object?)model.LicenseNumber ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@NeedLicense", model.NeedLicense);
                    cmdCtrl.Parameters.AddWithValue("@IsActive", model.IsActive);
                    cmdCtrl.Parameters.AddWithValue("@CurrentSalary", (object?)model.CurrentSalary ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@AnnualIncreasePercentage", (object?)model.AnnualIncreasePercentage ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@SalaryAfterAnnualIncrease", (object?)model.SalaryAfterAnnualIncrease ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@BankAccountNumber", (object?)model.BankAccountNumber ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@BankName", (object?)model.BankName ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@TaxId", (object?)model.TaxId ?? DBNull.Value);
                    cmdCtrl.Parameters.AddWithValue("@InsuranceNumber", (object?)model.InsuranceNumber ?? DBNull.Value);
                    cmdCtrl.ExecuteNonQuery();
                }

                tx.Commit();
            }
        }

        // Create success message with details about what was updated
        var successMessage = "Controller updated successfully!";
        if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            successMessage += " Photo updated.";
        if (model.LicenseFile != null && model.LicenseFile.Length > 0)
            successMessage += " License document updated.";
            
        TempData["SuccessMessage"] = successMessage;
        return RedirectToAction(nameof(Index));
    }


    // ?? ??? ???? LoadCountriesAndAirports ??? ?? ??? ?????? ??????

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        try
        {
            // Check for related data before attempting deletion
            var licenseCount = _db.GetLicenseCountByController(id);
            var certificateCount = _db.GetCertificateCountByController(id);
            var observationCount = _db.GetObservationCountByController(id);
            
            if (licenseCount > 0 || certificateCount > 0 || observationCount > 0)
            {
                var errorMessage = "Cannot delete controller because they have related data:";
                if (licenseCount > 0) errorMessage += $" {licenseCount} license(s),";
                if (certificateCount > 0) errorMessage += $" {certificateCount} certificate(s),";
                if (observationCount > 0) errorMessage += $" {observationCount} observation(s),";
                errorMessage = errorMessage.TrimEnd(',') + ". Please delete related data first.";
                
                TempData["Error"] = errorMessage;
                return RedirectToAction(nameof(Index));
            }

            // No related data exists, safe to delete
            _db.DeleteController(id);
            
            // Clean up notifications for deleted controller
            try
            {
                // Delete notifications related to this controller
                _db.ExecuteNonQuery("DELETE FROM notifications WHERE controllerid = @controllerId", 
                    new SqlParameter("@controllerId", id));
                
                // Update remaining notifications to remove orphaned references
                _db.ExecuteNonQuery(@"
                    UPDATE notifications 
                    SET controllerid = NULL 
                    WHERE controllerid NOT IN (SELECT controllerid FROM controllers WHERE controllerid IS NOT NULL)");
            }
            catch (Exception notificationEx)
            {
                // Log the error but don't fail the controller deletion
                Console.WriteLine($"Failed to clean up notifications: {notificationEx.Message}");
            }
            
            TempData["Success"] = "Controller has been deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error deleting controller: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }

    // ?? ??? ??? Controller ????? ?? (????? ControllerUserController.cs)

    // ???? ?? ???? using System.Linq; ?? ????? ?????

    public IActionResult ExportToPDF(string fullName, string username, string airportName, string icao_code,
    string jobTitle, string educationLevel, string maritalStatus, string phoneNumber, string email, string employmentStatus, string currentDepartment)
    {
        // 1. ????? ???????
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        // 2. ??? ???????? ????????
        var filteredControllers = _db.GetControllers(
            fullName, username, airportName, icao_code, jobTitle, educationLevel,
            maritalStatus, phoneNumber, email, employmentStatus, currentDepartment
        );

        // ==> ???? ??? ??? ??????? ??? <==
        var recordCount = filteredControllers.Count;

        // 3. ????? ???? ??????? (Styles)
        IContainer HeaderStyle(IContainer container) => container
            .Background(Colors.Blue.Medium)
            .PaddingVertical(4).PaddingHorizontal(6)
            .AlignCenter()
            .DefaultTextStyle(x => x.FontColor(Colors.White).FontSize(10).Bold());

        IContainer BodyCellStyle(IContainer container) => container
            .BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(4).PaddingHorizontal(6);

        // 4. ????? ???????
        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                // ????? ??? ?????? (Header)
                page.Header().Column(headerCol =>
                {
                    headerCol.Item().Row(row =>
                    {
                        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "carc.png");
                        if (System.IO.File.Exists(logoPath))
                        {
                            var logoBytes = System.IO.File.ReadAllBytes(logoPath);
                            row.ConstantColumn(80).Image(logoBytes).FitArea();
                        }

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().AlignCenter().Text("???? ????? ??????? ?????? ???????").Bold().FontSize(13);
                            col.Item().AlignCenter().Text("JORDAN CIVIL AVIATION REGULATORY COMMISSION").FontSize(10).FontColor(Colors.Grey.Darken1);
                            col.Item().PaddingTop(5).AlignCenter().Text(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).FontSize(9).FontColor(Colors.Grey.Darken2);

                        });
                    });
                    headerCol.Item().PaddingTop(15);
                    headerCol.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    headerCol.Item().PaddingTop(5);
                });

                // ????? ?????? (Content)
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderStyle).Text("#");
                        header.Cell().Element(HeaderStyle).Text("Full Name");
                        header.Cell().Element(HeaderStyle).Text("User Name");
                        header.Cell().Element(HeaderStyle).Text("Status");
                        header.Cell().Element(HeaderStyle).Text("Location");
                        header.Cell().Element(HeaderStyle).Text("Role");
                    });

                    int index = 1;
                    foreach (var c in filteredControllers)
                    {
                        table.Cell().Element(BodyCellStyle).AlignCenter().Text(index++.ToString());
                        table.Cell().Element(BodyCellStyle).Text(c.FullName ?? "-");
                        table.Cell().Element(BodyCellStyle).Text(c.Username ?? "-");
                        table.Cell().Element(BodyCellStyle).Text(c.EmploymentStatus ?? "-");
                        table.Cell().Element(BodyCellStyle).Text(c.AirportName ?? "-");
                        table.Cell().Element(BodyCellStyle).Text(c.Role ?? "-");
                    }
                });

                // =============================================================
                // ==> ??? ??????? ??????? ?????? ??? ??????? ?? ??????? <==
                // =============================================================
                page.Footer().Row(row =>
                {
                    // ?????? ????? ?? ????: ??? ??????? ??? ??????
                    row.RelativeColumn().Text(txt =>
                    {
                        txt.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1));
                        txt.Span($"Total Records: {recordCount}");
                    });

                    // ?????? ?????? ?? ????: ??? ?????? ??? ??????
                    row.RelativeColumn().AlignRight().Text(txt =>
                    {
                        txt.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1));
                        txt.Span("Page ");
                        txt.CurrentPageNumber();
                        txt.Span(" of ");
                        txt.TotalPages();
                    });
                });
            });
        });

        var pdfBytes = document.GeneratePdf();
        return File(pdfBytes, "application/pdf", $"Controllers_List_{DateTime.Now:yyyyMMdd}.pdf");
    }
    public IActionResult ExportToExcel(
    string fullName, string username, string airportName, string icao_code,
    string jobTitle, string educationLevel, string maritalStatus,
    string phoneNumber, string email, string employmentStatus, string currentDepartment)
    {
        // 1. ????? ????? ??????? ???????
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // 2. ??? ???????? ???????? ???? ???????
        var controllers = _db.GetControllers(
            fullName, username, airportName, icao_code, jobTitle, educationLevel,
            maritalStatus, phoneNumber, email, employmentStatus, currentDepartment
        );

        // 3. ????? ??? ??????
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Air Traffic Controllers");

            // --- ??????? ?????? ?????? ??????? ---
            worksheet.Cells.Style.Font.Name = "Arial";
            worksheet.View.RightToLeft = false; // ?????? ?? ?? ?????? ?? ?????? ??????

            // ????? ??????
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "carc.png");
            if (System.IO.File.Exists(logoPath))
            {
                var excelImage = worksheet.Drawings.AddPicture("Logo", logoPath);
                excelImage.SetPosition(0, 0, 0, 15); // (row, row offset, col, col offset)
                excelImage.SetSize(120, 65); // ????? ??? ?????? ????? ???????
            }

            // ????? ???????? ????????
            worksheet.Cells["C1"].Value = "???? ????? ??????? ?????? ???????";
            worksheet.Cells["C1"].Style.Font.Bold = true;
            worksheet.Cells["C1"].Style.Font.Size = 14;
            worksheet.Cells["C1:H1"].Merge = true;
            worksheet.Cells["C1:H1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["C2"].Value = "JORDAN CIVIL AVIATION REGULATORY COMMISSION";
            worksheet.Cells["C2"].Style.Font.Size = 10;
            worksheet.Cells["C2:H2"].Merge = true;
            worksheet.Cells["C2:H2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["C3"].Value = $"Controllers Report - {DateTime.Now:yyyy-MM-dd}";
            worksheet.Cells["C3"].Style.Font.Size = 9;
            worksheet.Cells["C3"].Style.Font.Italic = true;
            worksheet.Cells["C3:H3"].Merge = true;
            worksheet.Cells["C3:H3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // --- ????? ?????? ?????? ---
            var headers = new string[]
            {
            "#", "Full Name", "Username", "Airport", "ICAO", "Job Title",
            "Employment Status", "Department", "Role", "Hire Date", "Date of Birth",
            "Education", "Marital Status", "Phone Number", "Email"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[5, i + 1].Value = headers[i];
            }

            // ????? ?? ????????
            using (var range = worksheet.Cells[5, 1, 5, headers.Length])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#4F81BD")); // ??? ????
                range.Style.Font.Color.SetColor(Color.White);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // --- ????? ???????? ---
            int row = 6;
            int index = 1;
            foreach (var c in controllers)
            {
                worksheet.Cells[row, 1].Value = index++;
                worksheet.Cells[row, 2].Value = c.FullName;
                worksheet.Cells[row, 3].Value = c.Username;
                worksheet.Cells[row, 4].Value = c.AirportName;
                worksheet.Cells[row, 5].Value = c.icao_code;
                worksheet.Cells[row, 6].Value = c.JobTitle;
                worksheet.Cells[row, 7].Value = c.EmploymentStatus;
                worksheet.Cells[row, 8].Value = c.CurrentDepartment;
                worksheet.Cells[row, 9].Value = c.Role;
                worksheet.Cells[row, 10].Value = c.HireDate?.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 11].Value = c.DateOfBirth?.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 12].Value = c.EducationLevel;
                worksheet.Cells[row, 13].Value = c.MaritalStatus;
                worksheet.Cells[row, 14].Value = c.PhoneNumber;
                worksheet.Cells[row, 15].Value = c.Email;
                row++;
            }

            // ????? ??????? ??????? ??????????
            worksheet.Cells[6, 10, row - 1, 11].Style.Numberformat.Format = "yyyy-mm-dd";
            worksheet.Cells[6, 1, row - 1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // ??? ??????? ?????? ?? ??????? ????????
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var excelBytes = package.GetAsByteArray();
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Controllers_List_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
    // ?? NotificationsController ?????

    [HttpGet]
    public JsonResult GetAirports(int countryId)
    {
        var airports = _db.GetAirportsByCountry(countryId);
        return Json(airports);
    }

    // ????? ??? ?????? ??? ControllerUserController.cs

   [HttpGet]
public IActionResult ViewControllerDetails(int id)
{
    try
    {
        // ??? ???????? ???????? ???????
        var controllerData = _db.GetControllerById(id);
        if (controllerData.Rows.Count == 0)
        {
            return Json(new { success = false, message = "Controller not found" });
        }

        var row = controllerData.Rows[0];

        // ????? ???? ???????? ???????? ???????
        var controller = new ControllerUser
        {
            ControllerId = Convert.ToInt32(row["controllerid"]),
            FullName = row["fullname"].ToString(),
            Username = row["username"].ToString(),
            JobTitle = row["job_title"]?.ToString(),
            EducationLevel = row["education_level"]?.ToString(),
            DateOfBirth = row["date_of_birth"] as DateTime?,
            MaritalStatus = row["marital_status"]?.ToString(),
            PhoneNumber = row["phone_number"]?.ToString(),
            Email = row["email"]?.ToString(),
            Address = row["address"]?.ToString(),
            HireDate = row["hire_date"] as DateTime?,
            EmploymentStatus = row["employment_status"]?.ToString(),
            CurrentDepartment = row["current_department"]?.ToString(),
            EmergencyContact = row["emergency_contact"]?.ToString(),
            PhotoPath = row["photopath"]?.ToString(),
            LicenseNumber = row["LicenseNumber"]?.ToString(),
            // ?????? ???????
            NeedLicense = row["NeedLicense"] != DBNull.Value && Convert.ToBoolean(row["NeedLicense"]),
            IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]),
            CurrentSalary = row["CurrentSalary"] != DBNull.Value ? Convert.ToDecimal(row["CurrentSalary"]) : null,
            AnnualIncreasePercentage = row["AnnualIncreasePercentage"] != DBNull.Value ? Convert.ToDecimal(row["AnnualIncreasePercentage"]) : null,
            SalaryAfterAnnualIncrease = row["SalaryAfterAnnualIncrease"] != DBNull.Value ? Convert.ToDecimal(row["SalaryAfterAnnualIncrease"]) : null,
            BankAccountNumber = row["BankAccountNumber"]?.ToString(),
            BankName = row["BankName"]?.ToString(),
            TaxId = row["TaxId"]?.ToString(),
            InsuranceNumber = row["InsuranceNumber"]?.ToString()
        };

        // ??? ???????? ????????: ?????? ????????? ??????????
        var licenses = _db.GetLicensesByController(controller.Username);
        var certificates = _db.GetCertificatesByController(controller.Username);
        var observations = _db.GetObservationsByController(controller.Username);

        // --- ??? ?????? ???????? ??????? ---

        // 1. ??? ????? ???????? ???????? ???????
        var basicProjects = _db.GetProjectsByParticipant(controller.ControllerId, null);

        // 2. ????? ????? ?????? ???????? ?? ???????? ???????
        var detailedProjects = new List<object>();

        // 3. ?????? ??? ?? ????? ???? ??????? ????????
        foreach (var project in basicProjects)
        {
            // ??????? ?????? ???????? ???? ???? ????????
            var participants = _db.GetParticipantsByProjectId(project.ProjectId);
            var divisions = _db.GetDivisionsByProjectId(project.ProjectId);
            var files = _db.GetFilesByProjectId(project.ProjectId);

            // 4. ???? ?????? ???????? ???????
            detailedProjects.Add(new
            {
                id = project.ProjectId,
                projectName = project.ProjectName,
                status = project.Status,
                description = project.Description,
                location = project.Location,
                startDate = project.StartDate?.ToString("yyyy-MM-dd"),
                endDate = project.EndDate?.ToString("yyyy-MM-dd"),
                
                // ????? ??????? ?????????
                participants = participants.Select(p => new { name = p.Name, role = p.Role }).ToList(),
                divisions = divisions,
                files = files.Select(f => new { name = f.Name, size = f.Size, url = f.Url }).ToList()
            });
        }
        
        // --- ????? ????????? ???????? ---
        var response = new
        {
            success = true,
            controller = new
            {
                fullName = controller.FullName,
                username = controller.Username,
                jobTitle = controller.JobTitle,
                educationLevel = controller.EducationLevel,
                dateOfBirth = controller.DateOfBirth?.ToString("yyyy-MM-dd"),
                maritalStatus = controller.MaritalStatus,
                phoneNumber = controller.PhoneNumber,
                email = controller.Email,
                address = controller.Address,
                hireDate = controller.HireDate?.ToString("yyyy-MM-dd"),
                employmentStatus = controller.EmploymentStatus,
                currentDepartment = controller.CurrentDepartment,
                emergencyContact = controller.EmergencyContact,
                photoPath = Url.Content(controller.PhotoPath ?? "~/images/default-avatar.png"),
                licenseNumber = controller.LicenseNumber,
                // ?????? ???????
                needLicense = controller.NeedLicense,
                isActive = controller.IsActive,
                currentSalary = controller.CurrentSalary,
                annualIncreasePercentage = controller.AnnualIncreasePercentage,
                salaryAfterAnnualIncrease = controller.SalaryAfterAnnualIncrease,
                bankAccountNumber = controller.BankAccountNumber,
                bankName = controller.BankName,
                taxId = controller.TaxId,
                insuranceNumber = controller.InsuranceNumber
            },
            licenses = licenses.Select(l => new
            {
                typeName = l.TypeName,
                issueDate = l.IssueDate?.ToString("yyyy-MM-dd"),
                expiryDate = l.ExpiryDate?.ToString("yyyy-MM-dd"),
                filePath = Url.Content(l.FilePath ?? "#")
            }).ToList(),
            certificates = certificates.Select(c => new
            {
                typeName = c.TypeName,
                title = c.Title,
                issueDate = c.IssueDate?.ToString("yyyy-MM-dd"),
                expiryDate = c.ExpiryDate?.ToString("yyyy-MM-dd"),
                status = c.Status,
                filePath = Url.Content(c.FilePath ?? "#")
            }).ToList(),
            observations = observations.Select(o => new
            {
                travelCountry = o.TravelCountry,
                durationDays = o.DurationDays,
                departDate = o.DepartDate?.ToString("yyyy-MM-dd"),
                returnDate = o.ReturnDate?.ToString("yyyy-MM-dd"),
                licenseNumber = o.LicenseNumber,
                notes = o.Notes
            }).ToList(),
            // ??????? ????? ???????? ?????????
            projects = detailedProjects
        };

        return Json(response);
    }
    catch (Exception ex)
    {
        // ???? ????? ????? ??? ???????? ??? ??????
        // Log.Error(ex, "Error in ViewControllerDetails");
        return Json(new { success = false, message = "An error occurred while fetching details." });
    }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId) ? userId : 0;
    }

    // Import Controllers Actions
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> DownloadImportTemplate()
    {
        try
        {
            var excelService = new ExcelTemplateService();
            var templateBytes = excelService.GenerateControllersImportTemplate();
            
            return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Controllers_Import_Template.xlsx");
        }
        catch (Exception ex)
        {
            return BadRequest("Error generating template: " + ex.Message);
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ImportControllers(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file uploaded." });
            }

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
            {
                return Json(new { success = false, message = "Only Excel files (.xlsx, .xls) are supported." });
            }

            // Read Excel file
            var controllers = new List<ControllerImportModel>();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name == "Template Data");
                    if (worksheet == null)
                    {
                        return Json(new { success = false, message = "Invalid template format. 'Template Data' sheet not found." });
                    }

                    int rowCount = worksheet.Dimension?.Rows ?? 0;
                    if (rowCount < 2) // Header + at least one data row
                    {
                        return Json(new { success = false, message = "Template is empty or has no data rows." });
                    }

                    // Start from row 2 (skip header)
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var controller = new ControllerImportModel
                        {
                            FullName = worksheet.Cells[row, 1].Value?.ToString() ?? "",
                            Username = worksheet.Cells[row, 2].Value?.ToString() ?? "",
                            JobTitle = worksheet.Cells[row, 3].Value?.ToString() ?? "",
                            Division = worksheet.Cells[row, 4].Value?.ToString() ?? "",
                            OrganizationalStructure = worksheet.Cells[row, 5].Value?.ToString() ?? "",
                            ICAO = worksheet.Cells[row, 6].Value?.ToString() ?? "",
                            Email = worksheet.Cells[row, 7].Value?.ToString() ?? "",
                            PhoneNumber = worksheet.Cells[row, 8].Value?.ToString() ?? "",
                            CustomPassword = worksheet.Cells[row, 9].Value?.ToString() ?? "",
                            EducationLevel = worksheet.Cells[row, 10].Value?.ToString() ?? "",
                            DateOfBirth = ControllerImportModel.ParseExcelDate(worksheet.Cells[row, 11].Value),
                            MaritalStatus = worksheet.Cells[row, 12].Value?.ToString() ?? "",
                            Address = worksheet.Cells[row, 13].Value?.ToString() ?? "",
                            HireDate = ControllerImportModel.ParseExcelDate(worksheet.Cells[row, 14].Value),
                            EmploymentStatus = worksheet.Cells[row, 15].Value?.ToString() ?? "",
                            CurrentDepartment = worksheet.Cells[row, 16].Value?.ToString() ?? "",
                            NeedLicense = (worksheet.Cells[row, 17].Value?.ToString() ?? "No").Equals("Yes", StringComparison.OrdinalIgnoreCase),
                            IsActive = (worksheet.Cells[row, 18].Value?.ToString() ?? "Yes").Equals("Yes", StringComparison.OrdinalIgnoreCase)
                        };

                        // Validate required fields
                        if (string.IsNullOrWhiteSpace(controller.FullName) || 
                            string.IsNullOrWhiteSpace(controller.Username) ||
                            string.IsNullOrWhiteSpace(controller.JobTitle) ||
                            string.IsNullOrWhiteSpace(controller.Division) ||
                            string.IsNullOrWhiteSpace(controller.OrganizationalStructure) ||
                            string.IsNullOrWhiteSpace(controller.ICAO))
                        {
                            continue; // Skip invalid rows
                        }

                        controllers.Add(controller);
                    }
                }
            }

            if (controllers.Count == 0)
            {
                return Json(new { success = false, message = "No valid controller data found in the file." });
            }

            // Import controllers to database
            int successCount = 0;
            var errors = new List<string>();

            foreach (var controller in controllers)
            {
                try
                {
                    // Check if controller already exists
                    var existingController = _db.GetControllerByUsername(controller.Username);
                    if (existingController != null)
                    {
                        errors.Add($"Controller with username '{controller.Username}' already exists. Skipped.");
                        continue;
                    }

                    // Import controller using the new method
                    var result = _db.ImportControllerFromExcel(controller);
                    if (result)
                    {
                        successCount++;
                    }
                    else
                    {
                        errors.Add($"Failed to import controller '{controller.FullName}' ({controller.Username}).");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error importing controller '{controller.FullName}': {ex.Message}");
                }
            }

            var message = $"Successfully imported {successCount} controllers.";
            if (errors.Count > 0)
            {
                message += $" {errors.Count} errors occurred.";
            }

            return Json(new { 
                success = true, 
                message = message,
                importedCount = successCount,
                errorCount = errors.Count,
                errors = errors
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Import failed: " + ex.Message });
        }
    }


}
