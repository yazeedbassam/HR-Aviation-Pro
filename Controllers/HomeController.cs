using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using WebApplication1.DataAccess;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SqlServerDb _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, SqlServerDb db, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            // Debug: Check authentication status
            _logger.LogInformation($"🔍 HomeController - User authenticated: {User.Identity?.IsAuthenticated}");
            _logger.LogInformation($"🔍 HomeController - User name: {User.Identity?.Name}");
            _logger.LogInformation($"🔍 HomeController - User roles: {string.Join(", ", User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value))}");
            
            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //[HttpGet]
        //public IActionResult Profile()
        //{
        //    System.Diagnostics.Debugger.Break();

        //    // استخراج معرف المستخدم من الجلسة (ASP.NET Identity)
        //    string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (string.IsNullOrEmpty(userId))
        //        return RedirectToAction("Login", "Account");

        //    int currentUserId = int.Parse(userId);

        //    var table = _db.GetControllerById(currentUserId);
        //    if (table == null || table.Rows.Count == 0)
        //        return NotFound("User profile not found.");

        //    var currentUserData = table.Rows[0];
        //    string username = currentUserData["Username"].ToString();

        //    var model = new ProfileViewModel
        //    {
        //        ControllerId = currentUserId,
        //        FullName = currentUserData["FullName"]?.ToString(),
        //        Username = username,
        //        JobTitle = currentUserData["Job_Title"]?.ToString(),
        //        PhotoPath = currentUserData["PhotoPath"]?.ToString(),
        //        Email = currentUserData["Email"]?.ToString(),
        //        PhoneNumber = currentUserData["Phone_Number"]?.ToString(),
        //        DateOfBirth = currentUserData["Date_Of_Birth"] == DBNull.Value ? null : (DateTime?)currentUserData["Date_Of_Birth"],
        //        MaritalStatus = currentUserData["Marital_Status"]?.ToString(),
        //        Address = currentUserData["Address"]?.ToString(),
        //        HireDate = currentUserData["Hire_Date"] == DBNull.Value ? null : (DateTime?)currentUserData["Hire_Date"],
        //        EmploymentStatus = currentUserData["Employment_Status"]?.ToString(),
        //        CurrentDepartment = currentUserData["Current_Department"]?.ToString(),
        //        EducationLevel = currentUserData["Education_Level"]?.ToString(),
        //        EmergencyContact = currentUserData["Emergency_Contact"]?.ToString(),
        //        // القوائم والتبويبات
        //        MaritalStatuses = new SelectList(new List<string> { "Single", "Married", "Divorced", "Widowed" }, currentUserData["Marital_Status"]?.ToString()),
        //        Licenses = _db.GetLicensesByController(username),
        //        Certificates = _db.GetCertificatesByController(username),
        //        Observations = _db.GetObservationsByController(username),
        //    };

        //    // لعرض رسالة نجاح عند الرجوع من POST
        //    ViewBag.SuccessMessage = TempData["SuccessMessage"]?.ToString();
        //    return View(model);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Profile(ProfileViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var table = _db.GetControllerById(model.ControllerId);
        //        if (table == null || table.Rows.Count == 0)
        //            return NotFound("User not found.");

        //        var userData = table.Rows[0];

        //        // جلب الحقول الثابتة (غير القابلة للتعديل) كما هي من قاعدة البيانات
        //        string photoPath = userData["PhotoPath"]?.ToString();
        //        int airportId = Convert.ToInt32(userData["AirportId"]);
        //        string employmentStatus = userData["Employment_Status"]?.ToString();
        //        string currentDepartment = userData["Current_Department"]?.ToString();
        //        string jobTitle = userData["Job_Title"]?.ToString();
        //        DateTime? hireDate = userData["Hire_Date"] == DBNull.Value ? null : (DateTime?)userData["Hire_Date"];
        //        DateTime? dateOfBirth = userData["Date_Of_Birth"] == DBNull.Value ? null : (DateTime?)userData["Date_Of_Birth"];

        //        var updatedUser = new ControllerUser
        //        {
        //            ControllerId = model.ControllerId,
        //            FullName = model.FullName,
        //            Email = model.Email,
        //            MaritalStatus = model.MaritalStatus,
        //            PhoneNumber = model.PhoneNumber,
        //            Address = model.Address,
        //            EducationLevel = model.EducationLevel,
        //            EmergencyContact = model.EmergencyContact,
        //            Password = !string.IsNullOrEmpty(model.Password)
        //                ? BCrypt.Net.BCrypt.HashPassword(model.Password)
        //                : userData["Password"].ToString(),
        //            // الحقول الثابتة كما هي
        //            Username = model.Username,
        //            PhotoPath = photoPath,
        //            AirportId = airportId,
        //            EmploymentStatus = employmentStatus,
        //            CurrentDepartment = currentDepartment,
        //            JobTitle = jobTitle,
        //            HireDate = hireDate,
        //            DateOfBirth = dateOfBirth,
        //        };

        //        _db.UpdateController(updatedUser);

        //        TempData["SuccessMessage"] = "Profile updated successfully!";
        //        RedirectToAction("Profile", "Account");
        //    }

        //    // إعادة تعبئة الخصائص في حال وجود خطأ
        //    model.MaritalStatuses = new SelectList(new List<string> { "Single", "Married", "Divorced", "Widowed" }, model.MaritalStatus);
        //    model.Licenses = _db.GetLicensesByController(model.Username);
        //    model.Certificates = _db.GetCertificatesByController(model.Username);
        //    model.Observations = _db.GetObservationsByController(model.Username);

        //    return View(model);
        //}
    }
}
