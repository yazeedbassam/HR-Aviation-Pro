using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.IO; // <-- إضافة مهمة لاستخدام Path و File
using WebApplication1.DataAccess;
using WebApplication1.Models;
using WebApplication1.Services;
using Color = System.Drawing.Color;

namespace WebApplication1.Controllers;

[Authorize] // التحقق من الصلاحيات سيتم في كل action
public class EmployeesController : Controller
{
    private readonly SqlServerDb _db;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IConfigurationService _configurationService;

    public EmployeesController(SqlServerDb db, IWebHostEnvironment webHostEnvironment, IConfigurationService configurationService)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment; // قم بتعيينها هنا
        _configurationService = configurationService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return 0;
    }

    // GET: /Employees
    public async Task<IActionResult> Index()
    {
        // التحقق من صلاحية عرض الموظفين
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            return RedirectToAction("Login", "Account");
        }

        // التحقق من صلاحية عرض الموظفين
        var permissionService = HttpContext.RequestServices.GetService<IAdvancedPermissionManagerService>();
        if (permissionService != null)
        {
            var canViewEmployees = await permissionService.CanPerformOperationAsync(userId, "Employee", "View");
            if (!canViewEmployees)
            {
                return RedirectToAction("AccessDenied", "Account", new { returnUrl = "/Employees" });
            }
        }

        // نستدعي الدالة الجديدة بـ 8 قيم null لتتوافق مع تعريفها الجديد
        var employees = _db.GetEmployees(null, null, null, null, null, null, null, null, null);

        ViewBag.SectionTitle = "Employees & Operation Staff";
        ViewBag.SectionIcon = "fa-users";
        return View(employees);
    }

    // GET: /Employees/AIS
    public IActionResult AIS()
    {
        var employees = _db.GetEmployees(null, null, null, null, null, null, null, null, null)
                          .Where(e => e.Department?.Contains("AIS") == true)
                          .ToList();
        
        ViewBag.SectionTitle = "AIS - Aeronautical Information Services";
        ViewBag.SectionIcon = "bi-info-circle";
        return View("Index", employees);
    }

    // GET: /Employees/CNS
    public IActionResult CNS()
    {
        var employees = _db.GetEmployees(null, null, null, null, null, null, null, null, null)
                          .Where(e => e.Department?.Contains("CNS") == true)
                          .ToList();
        
        ViewBag.SectionTitle = "CNS - Communication, Navigation, and Surveillance";
        ViewBag.SectionIcon = "bi-wifi";
        return View("Index", employees);
    }

    // GET: /Employees/AFTN
    public IActionResult AFTN()
    {
        var employees = _db.GetEmployees(null, null, null, null, null, null, null, null, null)
                          .Where(e => e.Department?.Contains("AFTN") == true)
                          .ToList();
        
        ViewBag.SectionTitle = "AFTN - Aeronautical Fixed Telecommunication Network";
        ViewBag.SectionIcon = "bi-transmission";
        return View("Index", employees);
    }

    // GET: /Employees/OpsStaff
    public IActionResult OpsStaff()
    {
        var employees = _db.GetEmployees(null, null, null, null, null, null, null, null, null)
                          .Where(e => e.Department?.Contains("Administration") == true || 
                                     e.Department?.Contains("Safety") == true ||
                                     e.Department?.Contains("Quality") == true)
                          .ToList();
        
        ViewBag.SectionTitle = "Ops Staff & Administration";
        ViewBag.SectionIcon = "bi-gear";
        return View("Index", employees);
    }

    // POST: /Employees/Create
    // هذه الدالة وظيفتها استقبال البيانات بعد الضغط على زر "Create Employee"
    // في ملف EmployeesController.cs



    // =============================================================
    // IMPORT FROM EXCEL - POST
    // =============================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Please select an Excel file to import." });
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Please select a valid Excel file (.xlsx)." });
            }

            var importedCount = 0;
            var errors = new List<string>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        return Json(new { success = false, message = "No worksheet found in the Excel file." });
                    }

                    var rowCount = worksheet.Dimension?.Rows ?? 0;
                    if (rowCount < 2) // Header + at least one data row
                    {
                        return Json(new { success = false, message = "Excel file must contain at least one data row." });
                    }

                    // Process each row (skip header)
                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            var model = new EmployeeImportModel
                            {
                                FullName = worksheet.Cells[row, 1].Value?.ToString()?.Trim(),
                                Username = worksheet.Cells[row, 2].Value?.ToString()?.Trim(),
                                CustomPassword = worksheet.Cells[row, 3].Value?.ToString()?.Trim(),
                                EmployeeOfficialID = worksheet.Cells[row, 4].Value?.ToString()?.Trim(),
                                JobTitle = worksheet.Cells[row, 5].Value?.ToString()?.Trim(),
                                Department = worksheet.Cells[row, 6].Value?.ToString()?.Trim(),
                                Location = worksheet.Cells[row, 7].Value?.ToString()?.Trim(),
                                PhoneNumber = worksheet.Cells[row, 8].Value?.ToString()?.Trim(),
                                Email = worksheet.Cells[row, 9].Value?.ToString()?.Trim(),
                                Address = worksheet.Cells[row, 10].Value?.ToString()?.Trim(),
                                EmergencyContactPhone = worksheet.Cells[row, 11].Value?.ToString()?.Trim(),
                                Gender = worksheet.Cells[row, 12].Value?.ToString()?.Trim(),
                                DateOfBirth = ParseDate(worksheet.Cells[row, 13].Value?.ToString()),
                                MaritalStatus = worksheet.Cells[row, 14].Value?.ToString()?.Trim(),
                                EducationLevel = worksheet.Cells[row, 15].Value?.ToString()?.Trim(),
                                HireDate = ParseDate(worksheet.Cells[row, 16].Value?.ToString()),
                                CurrentSalary = ParseDecimal(worksheet.Cells[row, 17].Value?.ToString()),
                                AnnualIncreasePercentage = ParseDecimal(worksheet.Cells[row, 18].Value?.ToString()),
                                BankAccountNumber = worksheet.Cells[row, 19].Value?.ToString()?.Trim(),
                                BankName = worksheet.Cells[row, 20].Value?.ToString()?.Trim(),
                                TaxId = worksheet.Cells[row, 21].Value?.ToString()?.Trim(),
                                InsuranceNumber = worksheet.Cells[row, 22].Value?.ToString()?.Trim(),
                                OrganizationalStructure = worksheet.Cells[row, 23].Value?.ToString()?.Trim(),
                                Division = worksheet.Cells[row, 24].Value?.ToString()?.Trim(),
                                Role = worksheet.Cells[row, 25].Value?.ToString()?.Trim() ?? "Employee",
                                NeedLicense = ParseBool(worksheet.Cells[row, 26].Value?.ToString()) ?? true,
                                IsActive = ParseBool(worksheet.Cells[row, 27].Value?.ToString()) ?? true
                            };

                            // Validate required fields
                            if (string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.Username))
                            {
                                errors.Add($"Row {row}: Full Name and Username are required.");
                                continue;
                            }

                            // Import employee
                            if (_db.ImportEmployeeFromExcel(model))
                            {
                                importedCount++;
                            }
                            else
                            {
                                errors.Add($"Row {row}: Failed to import {model.FullName} ({model.Username}).");
                            }
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Row {row}: Error processing row - {ex.Message}");
                        }
                    }
                }
            }

            if (importedCount > 0)
            {
                return Json(new { success = true, message = $"Successfully imported {importedCount} employee(s). {errors.Count} error(s) occurred." });
            }

            if (errors.Any())
            {
                return Json(new { success = false, message = $"Import completed with {errors.Count} error(s). Check the logs for details." });
            }

            return Json(new { success = false, message = "No employees were imported." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error during import: {ex.Message}" });
        }
    }

    // =============================================================
    // DOWNLOAD EXCEL TEMPLATE
    // =============================================================
    [HttpGet]
    public IActionResult DownloadTemplate()
    {
        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Employee Template");
                
                // Headers
                var headers = new string[]
                {
                    "Full Name*", "Username*", "Custom Password", "Employee Official ID", "Job Title", 
                    "Department", "Location", "Phone Number", "Email", "Address", "Emergency Contact Phone",
                    "Gender", "Date of Birth", "Marital Status", "Education Level", "Hire Date",
                    "Current Salary", "Annual Increase %", "Bank Account Number", "Bank Name",
                    "Tax ID", "Insurance Number", "Organizational Structure", "Division", "Role", "Need License", "Is Active"
                };
                
                // Add headers
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }
                
                // Add sample data
                worksheet.Cells[2, 1].Value = "سارة أحمد";
                worksheet.Cells[2, 2].Value = "sara.ahmed";
                worksheet.Cells[2, 3].Value = "123";
                worksheet.Cells[2, 4].Value = "EMP001";
                worksheet.Cells[2, 5].Value = "Software Engineer";
                worksheet.Cells[2, 6].Value = "IT";
                worksheet.Cells[2, 7].Value = "Amman";
                worksheet.Cells[2, 8].Value = "+962-79-123-4567";
                worksheet.Cells[2, 9].Value = "john.doe@company.com";
                worksheet.Cells[2, 10].Value = "Amman, Jordan";
                worksheet.Cells[2, 11].Value = "+962-79-987-6543";
                worksheet.Cells[2, 12].Value = "Male";
                worksheet.Cells[2, 13].Value = "1990-05-15";
                worksheet.Cells[2, 14].Value = "Single";
                worksheet.Cells[2, 15].Value = "Bachelor's Degree";
                worksheet.Cells[2, 16].Value = "2023-01-01";
                worksheet.Cells[2, 17].Value = "5000.00";
                worksheet.Cells[2, 18].Value = "5.5";
                worksheet.Cells[2, 19].Value = "123456789";
                worksheet.Cells[2, 20].Value = "Bank of Jordan";
                worksheet.Cells[2, 21].Value = "TAX123456";
                worksheet.Cells[2, 22].Value = "INS789012";
                worksheet.Cells[2, 23].Value = "Jordan";
                worksheet.Cells[2, 24].Value = "Amman";
                worksheet.Cells[2, 25].Value = "Employee";
                worksheet.Cells[2, 26].Value = "Yes";
                worksheet.Cells[2, 27].Value = "Yes";
                
                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                
                var excelBytes = package.GetAsByteArray();
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Employees_Import_Template.xlsx");
            }
        }
        catch (Exception ex)
        {
            // Log error and return to index
            return RedirectToAction(nameof(Index));
        }
    }

    // =============================================================
    // Helper methods for parsing Excel data
    private DateTime? ParseDate(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        if (DateTime.TryParse(value, out DateTime result)) return result;
        return null;
    }

    private decimal? ParseDecimal(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        if (decimal.TryParse(value, out decimal result)) return result;
        return null;
    }

    private bool? ParseBool(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        if (bool.TryParse(value, out bool result)) return result;
        if (value.Equals("1", StringComparison.OrdinalIgnoreCase)) return true;
        if (value.Equals("0", StringComparison.OrdinalIgnoreCase)) return false;
        if (value.Equals("yes", StringComparison.OrdinalIgnoreCase)) return true;
        if (value.Equals("no", StringComparison.OrdinalIgnoreCase)) return false;
        return null;
    }

    // =============================================================
    // الدالة الأولى: لعرض الفورم الفارغ للمستخدم (GET)
    // لاحظ: لا يوجد متغيرات (parameters) في هذه الدالة
    // =============================================================
    public IActionResult Create()
    {
        // 1. نجهز القوائم المنسدلة من Configuration Service
        this.LoadConfigurationDropdown(_configurationService, "JobTitles", "JobTitles");
        this.LoadConfigurationDropdown(_configurationService, "Departments", "Departments");
        this.LoadConfigurationDropdown(_configurationService, "Gender", "Gender");
        
        // تحميل الأدوار من جدول Roles مع Description
        LoadRoles();

        // 2. تحميل الدول من قاعدة البيانات
        LoadCountries();

        // 3. نعرض الصفحة مع موديل فارغ
        return View(new CreateEmployeeViewModel());
    }


    // =================================================================
    // الدالة الثانية: لمعالجة البيانات بعد الضغط على زر الحفظ (POST)
    // لاحظ: هذه الدالة تستقبل الموديل ولديها [HttpPost]
    // =================================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateEmployeeViewModel model, IFormFile? photoFile)
    {
        // التحقق من اسم المستخدم والإيميل
        if (_db.GetUserByUsername(model.Username) != null)
        {
            ModelState.AddModelError("Username", "This username is already taken.");
        }
        //if (_db.EmployeeEmailExists(model.Email))
        //{
        //    ModelState.AddModelError("Email", "This email address is already in use.");
        //}

        // التحقق من وجود الدور في جدول Roles
        if (string.IsNullOrEmpty(model.RoleName))
        {
            ModelState.AddModelError("RoleName", "Role is required.");
        }
        else
        {
            // Check if role exists in Roles table
            var roleExists = _db.ExecuteQuery("SELECT COUNT(*) FROM Roles WHERE RoleName = @RoleName", 
                new SqlParameter("@RoleName", model.RoleName));
            
            if (Convert.ToInt32(roleExists.Rows[0][0]) == 0)
            {
                ModelState.AddModelError("RoleName", $"Role '{model.RoleName}' does not exist in the system.");
            }
        }

        if (ModelState.IsValid) // هذا الشرط سيتحقق الآن من الأخطاء التي أضفتها يدوياً
        {
            try
            {
                // Handle photo upload
                if (photoFile != null && photoFile.Length > 0)
                {
                    string photoPath = SaveUploadedFile(photoFile, "employees", "photos", $"employee_{DateTime.Now:yyyyMMddHHmmss}");
                    model.PhotoPath = photoPath;
                }

                _db.CreateEmployeeAndUser(model);
                TempData["SuccessMessage"] = "Employee created successfully!";
                
                // توجيه المستخدم إلى القسم المناسب حسب نوع الموظف
                if (model.Department?.Contains("AIS") == true)
                {
                    return RedirectToAction("AIS");
                }
                else if (model.Department?.Contains("CNS") == true)
                {
                    return RedirectToAction("CNS");
                }
                else if (model.Department?.Contains("AFTN") == true)
                {
                    return RedirectToAction("AFTN");
                }
                else if (model.Department?.Contains("Administration") == true || 
                         model.Department?.Contains("Safety") == true ||
                         model.Department?.Contains("Quality") == true)
                {
                    return RedirectToAction("OpsStaff");
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred: " + ex.Message);
            }
        }

        // في حالة الفشل، نعيد تعبئة القوائم المنسدلة من Configuration Service
        this.LoadConfigurationDropdown(_configurationService, "JobTitles", "JobTitles");
        this.LoadConfigurationDropdown(_configurationService, "Departments", "Departments");
        this.LoadConfigurationDropdown(_configurationService, "Gender", "Gender");
        
        // تحميل الأدوار من جدول Roles مع Description
        LoadRoles();

        // إعادة تحميل الدول
        LoadCountries();

        return View(model);
    }
    // GET: /Employees/Edit/5
    public IActionResult Edit(int id)
    {
        // Load dropdowns from Configuration Service
        this.LoadConfigurationDropdown(_configurationService, "JobTitles", "JobTitles");
        this.LoadConfigurationDropdown(_configurationService, "Departments", "Departments");
        this.LoadConfigurationDropdown(_configurationService, "Gender", "Gender");
        
        // تحميل الأدوار من جدول Roles مع Description
        LoadRoles();
        
        // تحميل الدول من قاعدة البيانات
        LoadCountries();
        
        var employee = _db.GetEmployeeById(id);
        if (employee == null)
        {
            return NotFound();
        }

        // تحميل المطارات إذا كان هناك organizational structure محدد
        if (!string.IsNullOrEmpty(employee.OrganizationalStructure))
        {
            LoadAirportsForCountry(employee.OrganizationalStructure);
        }

        return View(employee);
    }

    // POST: /Employees/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Employee employee, IFormFile? photoFile, string? NewPassword, string? Role)
    {
        if (id != employee.EmployeeID)
        {
            return BadRequest();
        }

        // إزالة حقول معينة من التحقق لأنها غير موجودة في الفورم
        ModelState.Remove("EmployeeOfficialID");
        ModelState.Remove("Email");

        if (ModelState.IsValid)
        {
            try
            {
                // Handle photo upload
                if (photoFile != null && photoFile.Length > 0)
                {
                    string photoPath = SaveUploadedFile(photoFile, "employees", "photos", $"employee_{employee.EmployeeID}_{DateTime.Now:yyyyMMddHHmmss}");
                    employee.PhotoPath = photoPath;
                }

                // تحديث كلمة المرور إذا تم توفيرها وغير فارغة
                if (!string.IsNullOrEmpty(NewPassword) && NewPassword.Trim().Length > 0)
                {
                    _db.UpdateUserPassword(employee.Username, NewPassword);
                }

                // تحديث Role إذا تم توفيره
                if (!string.IsNullOrEmpty(Role))
                {
                    employee.Role = Role;
                }

                _db.UpdateEmployee(employee);
                TempData["SuccessMessage"] = "Employee updated successfully!";
                
                // توجيه المستخدم إلى القسم المناسب حسب نوع الموظف
                if (employee.Department?.Contains("AIS") == true)
                {
                    return RedirectToAction("AIS");
                }
                else if (employee.Department?.Contains("CNS") == true)
                {
                    return RedirectToAction("CNS");
                }
                else if (employee.Department?.Contains("AFTN") == true)
                {
                    return RedirectToAction("AFTN");
                }
                else if (employee.Department?.Contains("Administration") == true || 
                         employee.Department?.Contains("Safety") == true ||
                         employee.Department?.Contains("Quality") == true)
                {
                    return RedirectToAction("OpsStaff");
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while updating the employee: " + ex.Message);
            }
        }

        // في حالة الفشل، نعيد تحميل البيانات
        this.LoadConfigurationDropdown(_configurationService, "JobTitles", "JobTitles");
        this.LoadConfigurationDropdown(_configurationService, "Departments", "Departments");
        this.LoadConfigurationDropdown(_configurationService, "Gender", "Gender");
        
        // تحميل الأدوار من جدول Roles مع Description
        LoadRoles();
        
        LoadCountries();
        
        if (!string.IsNullOrEmpty(employee.OrganizationalStructure))
        {
            LoadAirportsForCountry(employee.OrganizationalStructure);
        }

        return View(employee);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        try
        {
            // نتأكد أولاً أن الموظف موجود (خطوة اختيارية لكنها جيدة)
            var employee = _db.GetEmployeeById(id);
            if (employee == null)
            {
                TempData["Error"] = "Employee not found.";
                return RedirectToAction(nameof(Index));
            }

            // حفظ القسم قبل الحذف
            var department = employee.Department;

            // استدعاء ميثود الحذف الجديدة
            _db.DeleteEmployee(id);

            TempData["SuccessMessage"] = "Employee has been deleted successfully.";
            
            // توجيه المستخدم إلى القسم المناسب حسب نوع الموظف المحذوف
            if (department?.Contains("AIS") == true)
            {
                return RedirectToAction("AIS");
            }
            else if (department?.Contains("CNS") == true)
            {
                return RedirectToAction("CNS");
            }
            else if (department?.Contains("AFTN") == true)
            {
                return RedirectToAction("AFTN");
            }
            else if (department?.Contains("Administration") == true || 
                     department?.Contains("Safety") == true ||
                     department?.Contains("Quality") == true)
            {
                return RedirectToAction("OpsStaff");
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            // في حال حدوث أي خطأ، نعرض رسالة خطأ
            TempData["Error"] = $"Error deleting employee: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
    // أضف هذه الدوال داخل EmployeesController.cs

    public IActionResult ExportToPDF(string fullName, string employeeOfficialID, string jobTitle, string department, string username)
    {
        // 1. تحديد الترخيص
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        // 2. جلب البيانات المفلترة
        var filteredEmployees = _db.GetEmployees(fullName, employeeOfficialID, jobTitle, department, username, null, null, null, null);
        var recordCount = filteredEmployees.Count;

        // 3. تعريف دوال التنسيق (Styles)
        IContainer HeaderStyle(IContainer container) => container
            .Background(Colors.Blue.Medium)
            .PaddingVertical(4).PaddingHorizontal(6)
            .AlignCenter()
            .DefaultTextStyle(x => x.FontColor(Colors.White).FontSize(9).Bold()); // تصغير خط الهيدر

        IContainer BodyCellStyle(IContainer container) => container
            .BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(4).PaddingHorizontal(6)
            .DefaultTextStyle(x => x.FontSize(8)); // تصغير خط المحتوى

        // 4. إنشاء المستند
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre); // تقليل الهوامش قليلاً
                page.DefaultTextStyle(x => x.FontFamily("Arial"));

                // تصميم رأس الصفحة (Header)
                page.Header().Column(headerCol =>
                {
                    headerCol.Item().Row(row =>
                    {
                        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "carc.png");
                        if (System.IO.File.Exists(logoPath))
                        {
                            var logoBytes = System.IO.File.ReadAllBytes(logoPath);
                            row.ConstantColumn(70).Image(logoBytes); // تصغير حجم الشعار قليلاً
                        }

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().AlignCenter().Text("هيئة تنظيم الطيران المدني الأردني").Bold().FontSize(12); // تصغير الخط
                            col.Item().AlignCenter().Text("JORDAN CIVIL AVIATION REGULATORY COMMISSION").FontSize(9).FontColor(Colors.Grey.Darken1); // تصغير الخط
                            col.Item().PaddingTop(5).AlignCenter().Text($"Employees Report - {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken2); // تصغير الخط
                        });
                    });
                    headerCol.Item().PaddingTop(10); // تقليل المسافة
                    headerCol.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    headerCol.Item().PaddingTop(5);
                });

                // محتوى الصفحة (Content)
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25);         // #
                        columns.RelativeColumn(2.5f);       // Full Name
                        columns.RelativeColumn(1.5f);       // Employee ID
                        columns.RelativeColumn(2f);         // Job Title
                        columns.RelativeColumn(2.5f);       // Department
                        columns.RelativeColumn(1.5f);       // Username
                        columns.RelativeColumn(1f);         // Status
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderStyle).Text("#");
                        header.Cell().Element(HeaderStyle).Text("Full Name");
                        header.Cell().Element(HeaderStyle).Text("User ID");
                        header.Cell().Element(HeaderStyle).Text("Job Title");
                        header.Cell().Element(HeaderStyle).Text("Department");
                        header.Cell().Element(HeaderStyle).Text("Username");
                        header.Cell().Element(HeaderStyle).Text("Status");
                    });

                    int index = 1;
                    foreach (var emp in filteredEmployees)
                    {
                        table.Cell().Element(BodyCellStyle).AlignCenter().Text(index++.ToString());
                        table.Cell().Element(BodyCellStyle).Text(emp.FullName ?? "-");
                        table.Cell().Element(BodyCellStyle).Text(emp.EmployeeOfficialID ?? "-");
                        table.Cell().Element(BodyCellStyle).Text(emp.JobTitle ?? "-");
                        table.Cell().Element(BodyCellStyle).Text(emp.Department ?? "-");
                        table.Cell().Element(BodyCellStyle).Text(emp.Username ?? "-");
                        table.Cell().Element(BodyCellStyle).Text(emp.IsActive ? "Active" : "Inactive");
                    }
                });

                // تصميم تذييل الصفحة (Footer)
                page.Footer().Row(row =>
                {
                    row.RelativeColumn().Text(txt =>
                    {
                        txt.DefaultTextStyle(x => x.FontSize(7).FontColor(Colors.Grey.Darken1)); // تصغير الخط
                        txt.Span($"Total Records: {recordCount}");
                    });

                    row.RelativeColumn().AlignRight().Text(txt =>
                    {
                        txt.DefaultTextStyle(x => x.FontSize(7).FontColor(Colors.Grey.Darken1)); // تصغير الخط
                        txt.Span("Page ");
                        txt.CurrentPageNumber();
                        txt.Span(" of ");
                        txt.TotalPages();
                    });
                });
            });
        });

        var pdfBytes = document.GeneratePdf();
        return File(pdfBytes, "application/pdf", $"Employees_List_{DateTime.Now:yyyyMMdd}.pdf");
    }


    public IActionResult ExportToExcel(
        string fullName, string employeeOfficialID, string jobTitle, string department,
        string username, string phoneNumber, string email, string location, string? gender)
    {
        // 1. تحديد ترخيص استخدام المكتبة
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // 2. جلب البيانات المفلترة بنفس الطريقة
        var employees = _db.GetEmployees(
            fullName, employeeOfficialID, jobTitle, department, username,
            phoneNumber, email, location, gender
        );

        // 3. إنشاء ملف الإكسل
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Employees");

            // --- إعدادات وتصميم الهيدر والشعار ---
            worksheet.Cells.Style.Font.Name = "Arial";
            worksheet.View.RightToLeft = false; // للتأكد من أن الورقة من اليسار لليمين

            // إضافة الشعار
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "carc.png");
            if (System.IO.File.Exists(logoPath))
            {
                var excelImage = worksheet.Drawings.AddPicture("Logo", logoPath);
                excelImage.SetPosition(0, 0, 0, 15); // (row, row offset, col, col offset)
                excelImage.SetSize(120, 65); // تعديل حجم الشعار ليكون مناسبًا
            }

            // إضافة العناوين الرئيسية
            worksheet.Cells["C1"].Value = "هيئة تنظيم الطيران المدني الأردني";
            worksheet.Cells["C1"].Style.Font.Bold = true;
            worksheet.Cells["C1"].Style.Font.Size = 14;
            worksheet.Cells["C1:H1"].Merge = true;
            worksheet.Cells["C1:H1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["C2"].Value = "JORDAN CIVIL AVIATION REGULATORY COMMISSION";
            worksheet.Cells["C2"].Style.Font.Size = 10;
            worksheet.Cells["C2:H2"].Merge = true;
            worksheet.Cells["C2:H2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["C3"].Value = $"Employees Report - {DateTime.Now:yyyy-MM-dd}";
            worksheet.Cells["C3"].Style.Font.Size = 9;
            worksheet.Cells["C3"].Style.Font.Italic = true;
            worksheet.Cells["C3:H3"].Merge = true;
            worksheet.Cells["C3:H3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // --- تحديد عناوين الجدول ---
            var headers = new string[]
            {
            "#", "Full Name", "User ID", "Job Title", "Department", "Hire Date", "Gender",
            "Email", "Phone Number", "Emergency Contact", "Location", "Address", "Status", "Username"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[5, i + 1].Value = headers[i];
            }

            // تنسيق صف العناوين
            using (var range = worksheet.Cells[5, 1, 5, headers.Length])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#4F81BD")); // لون أزرق
                range.Style.Font.Color.SetColor(Color.White);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // --- إضافة البيانات ---
            int row = 6;
            int index = 1;
            foreach (var emp in employees)
            {
                worksheet.Cells[row, 1].Value = index++;
                worksheet.Cells[row, 2].Value = emp.FullName;
                worksheet.Cells[row, 3].Value = emp.EmployeeOfficialID;
                worksheet.Cells[row, 4].Value = emp.JobTitle;
                worksheet.Cells[row, 5].Value = emp.Department;
                worksheet.Cells[row, 6].Value = emp.HireDate?.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 7].Value = emp.Gender;
                worksheet.Cells[row, 8].Value = emp.Email;
                worksheet.Cells[row, 9].Value = emp.PhoneNumber;
                worksheet.Cells[row, 10].Value = emp.EmergencyContactPhone;
                worksheet.Cells[row, 11].Value = emp.Location;
                worksheet.Cells[row, 12].Value = emp.Address;
                worksheet.Cells[row, 13].Value = emp.IsActive ? "Active" : "Inactive";
                worksheet.Cells[row, 14].Value = emp.Username;
                row++;
            }

            // تنسيق الخلايا الرقمية والتاريخية
            worksheet.Cells[6, 6, row - 1, 6].Style.Numberformat.Format = "yyyy-mm-dd";
            worksheet.Cells[6, 1, row - 1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // جعل الأعمدة تتناسب مع المحتوى تلقائيًا
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var excelBytes = package.GetAsByteArray();
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Employees_List_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }

    [HttpGet]
    public IActionResult ViewEmployeeDetails(int id)
    {
        try
        {
            // جلب بيانات الموظف الأساسية
            var employee = _db.GetEmployeeById(id);
            if (employee == null)
            {
                return Json(new { success = false, message = "Employee not found" });
            }

            // جلب الرخص الخاصة بالموظف
            var licenses = _db.GetLicensesByEmployeeId(id).Select(l => new
            {
                typeName = l.TypeName,
                issueDate = l.IssueDate?.ToString("yyyy-MM-dd"),
                expiryDate = l.ExpiryDate?.ToString("yyyy-MM-dd"),
                status = l.Status,
                filePath = !string.IsNullOrEmpty(l.FilePath) ? l.FilePath : "#"
            }).ToList();

            // جلب الشهادات الخاصة بالموظف
            var certificates = _db.GetCertificatesByEmployeeId(id).Select(c => new
            {
                typeName = c.TypeName,
                title = c.Title,
                issueDate = c.IssueDate?.ToString("yyyy-MM-dd"),
                expiryDate = c.ExpiryDate?.ToString("yyyy-MM-dd"),
                status = c.Status,
                filePath = !string.IsNullOrEmpty(c.FilePath) ? c.FilePath : "#"
            }).ToList();

            // جلب الملاحظات/السفرات الخاصة بالموظف
            var observations = _db.GetObservationsByEmployeeId(id).Select(o => new
            {
                travelCountry = o.TravelCountry,
                durationDays = o.DurationDays,
                departDate = o.DepartDate?.ToString("yyyy-MM-dd"),
                returnDate = o.ReturnDate?.ToString("yyyy-MM-dd"),
                licenseNumber = o.LicenseNumber,
                notes = o.Notes
            }).ToList();

            // جلب المشاريع التي يشارك فيها الموظف
            var projects = _db.GetProjectsByEmployeeId(id).Select(p => new
            {
                id = p.ProjectId,
                projectName = p.ProjectName,
                description = p.Description,
                startDate = p.StartDate?.ToString("yyyy-MM-dd"),
                endDate = p.EndDate?.ToString("yyyy-MM-dd"),
                location = p.Location,
                status = p.Status,
                participants = _db.GetParticipantsByProjectId(p.ProjectId).Select(participant => new
                {
                    name = participant.Name,
                    role = participant.Role
                }).ToList(),
                divisions = _db.GetDivisionsByProjectId(p.ProjectId),
                files = GetProjectFiles(p.FolderPath ?? "")
            }).ToList();

            // إعداد البيانات للإرسال
            var result = new
            {
                success = true,
                employee = new
                {
                    fullName = employee.FullName,
                    username = employee.Username,
                    email = employee.Email,
                    phoneNumber = employee.PhoneNumber,
                    dateOfBirth = employee.DateOfBirth?.ToString("yyyy-MM-dd"),
                    maritalStatus = employee.MaritalStatus,
                    currentDepartment = employee.Department,
                    employmentStatus = employee.IsActive ? "Active" : "Inactive",
                    hireDate = employee.HireDate?.ToString("yyyy-MM-dd"),
                    educationLevel = employee.EducationLevel,
                    address = employee.Address,
                    emergencyContact = employee.EmergencyContactPhone,
                    jobTitle = employee.JobTitle,
                    gender = employee.Gender,
                    employeeOfficialID = employee.EmployeeOfficialID,
                    photoPath = !string.IsNullOrEmpty(employee.PhotoPath) ? employee.PhotoPath : "/images/default-avatar.png",
                    // Financial Information
                    currentSalary = employee.CurrentSalary,
                    annualIncreasePercentage = employee.AnnualIncreasePercentage,
                    salaryAfterAnnualIncrease = employee.SalaryAfterAnnualIncrease,
                    bankAccountNumber = employee.BankAccountNumber,
                    bankName = employee.BankName,
                    taxId = employee.TaxId,
                    insuranceNumber = employee.InsuranceNumber,
                    // Organizational Structure
                    organizationalStructure = employee.OrganizationalStructure,
                    division = employee.Division
                },
                licenses = licenses,
                certificates = certificates,
                observations = observations,
                projects = projects
            };

            return Json(result);
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // دالة مساعدة لجلب ملفات المشروع
    private List<object> GetProjectFiles(string folderPath)
    {
        var files = new List<object>();

        if (string.IsNullOrEmpty(folderPath))
            return files;

        try
        {
            string physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, folderPath.TrimStart('/', '\\'));
            if (Directory.Exists(physicalPath))
            {
                var directoryInfo = new DirectoryInfo(physicalPath);
                foreach (var file in directoryInfo.GetFiles().OrderBy(f => f.Name))
                {
                    files.Add(new
                    {
                        name = file.Name,
                        url = $"{folderPath}/{file.Name}".Replace('\\', '/'),
                        size = FormatFileSize(file.Length)
                    });
                }
            }
        }
        catch (Exception)
        {
            // في حالة حدوث خطأ، نعيد قائمة فارغة
        }

        return files;
    }

    // دالة مساعدة لتنسيق حجم الملف
    private string FormatFileSize(long bytes)
    {
        var unit = 1024;
        if (bytes < unit) return $"{bytes} B";
        var exp = (int)(Math.Log(bytes) / Math.Log(unit));
        var pre = "KMGTPE"[exp - 1];
        return $"{bytes / Math.Pow(unit, exp):F1} {pre}B";
    }

    // دالة مساعدة لحفظ الملفات المرفوعة
    private string SaveUploadedFile(IFormFile file, string folderCategory, string userFolder, string defaultFileName)
    {
        if (file == null || file.Length == 0)
            return string.Empty;

        try
        {
            // إنشاء مجلد الحفظ
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folderCategory, userFolder);
            Directory.CreateDirectory(uploadsFolder);

            // إنشاء اسم الملف الفريد
            string fileName = $"{defaultFileName}{Path.GetExtension(file.FileName)}";
            string filePath = Path.Combine(uploadsFolder, fileName);

            // حفظ الملف
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // إرجاع المسار النسبي للملف
            return $"/uploads/{folderCategory}/{userFolder}/{fileName}";
        }
        catch (Exception)
        {
            // في حالة حدوث خطأ، نعيد مسار فارغ
            return string.Empty;
        }
    }
    public IActionResult Details(int id)
    {
        var viewModel = _db.GetEmployeeDetailsById(id);
        if (viewModel == null)
        {
            return NotFound();
        }
        return View(viewModel);
    }

    // دالة تحميل الدول من قاعدة البيانات
    private void LoadCountries()
    {
        var dtC = _db.ExecuteQuery("SELECT countryid, countryname FROM countries ORDER BY countryname");
        ViewBag.Countries = dtC.Rows.Cast<DataRow>()
            .Select(r => new SelectListItem
            {
                Value = r["countryid"].ToString(),
                Text = r["countryname"].ToString()
            }).ToList();
    }

         // دالة تحميل المطارات لدولة معينة
     private void LoadAirportsForCountry(string countryName)
     {
         // البحث عن الدولة بالاسم أولاً
         var countryQuery = "SELECT countryid FROM countries WHERE countryname = @countryName";
         var countryResult = _db.ExecuteQuery(countryQuery, new SqlParameter("@countryName", countryName));
         
         if (countryResult.Rows.Count > 0)
         {
             var countryId = countryResult.Rows[0]["countryid"].ToString();
             
             var dtA = _db.ExecuteQuery(
                 "SELECT airportid, airportname, icao_code FROM airports WHERE countryid = @countryId ORDER BY airportname",
                 new SqlParameter("@countryId", countryId));
             
             ViewBag.Airports = dtA.Rows.Cast<DataRow>()
                 .Select(r => new SelectListItem
                 {
                     Value = r["airportid"].ToString(),
                     Text = $"{r["airportname"]} ({r["icao_code"]})"
                 }).ToList();
         }
         else
         {
             ViewBag.Airports = new List<SelectListItem>();
         }
     }

    /// <summary>
    /// تحميل الأدوار من جدول Roles مع Description
    /// </summary>
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

}