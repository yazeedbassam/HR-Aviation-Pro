using DocuSign.eSign.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Data;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication1.DataAccess;
using WebApplication1.Models;
using static System.Formats.Asn1.AsnWriter;

public class AccountController : Controller
{
    private readonly SqlServerDb _db;
    private readonly string _brevoSmtpServer = "smtp-relay.brevo.com";
    private readonly int _brevoSmtpPort = 587;
    private readonly string _brevoLogin = "8e2caf001@smtp-brevo.com";
    private readonly string _brevoPassword = "3HzgVG7nwKMxqcA2";
    private readonly string _fromEmail = "yazeedbassam1987@gmail.com";

    public AccountController(SqlServerDb db) => _db = db;

    public IActionResult Notifications()
    {
        DataTable dt = new DataTable();
        string sql = "";

        try
        {
            if (User.IsInRole("Admin"))
            {
                // استعلام موحد للمسؤول يجلب المراقبين والموظفين مع حساب الأيام المتبقية
                sql = @"
                SELECT
                    'Controller' AS UserType,
                    l.licensetype,
                    l.expirydate AS licenseexpirydate,
                    DATEDIFF(day, GETDATE(), l.expirydate) AS RemainingDays,
                    c.fullname AS ControllerName,
                    c.phone_number,
                    c.email,
                    c.current_department AS Department,
                    a.airportname
                FROM licenses l
                INNER JOIN controllers c ON l.controllerid = c.controllerid
                INNER JOIN airports a ON c.airportid = a.airportid
                WHERE l.expirydate <= DATEADD(day,60, GETDATE())

                UNION ALL

                SELECT
                    e.department AS UserType,
                    l.licensetype,
                    l.expirydate AS licenseexpirydate,
                    DATEDIFF(day, GETDATE(), l.expirydate) AS RemainingDays,
                    e.fullname AS ControllerName,
                    e.phonenumber AS phone_number,
                    e.email,
                    e.department AS Department,
                    'HQ - Main Office' AS airportname
                FROM licenses l
                INNER JOIN employees e ON l.employeeid = e.employeeid
                WHERE l.expirydate <= DATEADD(day, 60, GETDATE())
                ORDER BY RemainingDays ASC;";

                dt = _db.ExecuteQuery(sql);
            }
            else
            {
                // استعلام موحد للمشرفين والمستخدمين الآخرين
                // يتم تحديد القسم بناءً على دور المشرف
                string departmentFilter = "";
                if (User.IsInRole("SuperVisor OJAI"))
                {
                    departmentFilter = "WHERE c.Current_Department = 'Queen'";
                }
                else if (User.IsInRole("SuperVisor OJAM"))
                {
                    departmentFilter = "WHERE c.Current_Department = 'AMMAN'";
                }
                else if (User.IsInRole("SuperVisor TACC"))
                {
                    departmentFilter = "WHERE c.Current_Department = 'TACC'";
                }
                else // للمستخدم العادي (غير المسؤول أو المشرف)
                {
                    // هذا الجزء يعتمد على كيفية ربط المستخدم العادي بالإشعارات
                    // افترض أنه مرتبط بـ userid
                    int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    departmentFilter = $"WHERE n.userid = {userId}";
                }

                sql = $@"
                SELECT
                    'Controller' AS UserType,
                    n.licensetype,
                    n.licenseexpirydate,
                    DATEDIFF(day, GETDATE(), n.licenseexpirydate) AS RemainingDays,
                    c.fullname AS ControllerName,
                    c.phone_number,
                    c.email,
                    c.current_department AS Department,
                    a.airportname
                FROM notifications n
                INNER JOIN controllers c ON n.controllerid = c.controllerid
                JOIN airports a ON a.airportid = c.airportid
                {departmentFilter}
                ORDER BY RemainingDays ASC, n.created_at DESC;";

                dt = _db.ExecuteQuery(sql);
            }

            // الآن لا حاجة لحساب الأيام المتبقية في C# لأنها حُسبت في SQL
            // ولا حاجة للترتيب هنا لأنه تم في SQL
            return View("Notifications", dt);
        }
        catch (Exception ex)
        {
            // Log the error
            // Consider returning an error view
            return View("Error"); // Make sure you have an Error.cshtml view
        }
    }

    [HttpPost]
    public async Task<IActionResult> SendExpiryEmails()
    {
        var expiredLicenses = _db.ExecuteQuery(@"
        SELECT l.controllerid, c.email, l.licensetype, l.expirydate, c.fullname
            FROM licenses l
            INNER JOIN controllers c ON l.controllerid = c.controllerid
            WHERE    l.expirydate BETWEEN GETDATE() AND DATEADD(day, 30, GETDATE());
    ");

        if (expiredLicenses.Rows.Count > 0)
        {

            foreach (DataRow row in expiredLicenses.Rows)
            {
                int controllerId = Convert.ToInt32(row["controllerid"]);
                string toEmail = row["email"].ToString();
                string licenseType = row["licensetype"].ToString();
                DateTime expiryDate = Convert.ToDateTime(row["expirydate"]);
                string fullname = row["fullname"].ToString();
                string subject = "Notify: اقتراب انتهاء صلاحية الرخصة (license expire)";
                string body = $"Dear {fullname}, Your {licenseType} will expire :  At {expiryDate:yyyy-MM-dd} 😊 \n\n So, Please Update 😔. \n\nيرجى اتخاذ الإجراءات اللازمة لتجديدها.";



                if (!string.IsNullOrWhiteSpace(toEmail))
                {
                    try
                    {
                        using (var smtp = new SmtpClient(_brevoSmtpServer))
                        {
                            smtp.Port = _brevoSmtpPort;
                            smtp.Credentials = new System.Net.NetworkCredential(_brevoLogin, _brevoPassword);
                            smtp.EnableSsl = true;
                            using (var mail = new MailMessage(_fromEmail, toEmail, subject, body))
                            {
                                await smtp.SendMailAsync(mail);
                                Console.WriteLine($"تم إرسال بريد إلكتروني بشأن انتهاء الصلاحية إلى {toEmail} عبر Brevo SMTP");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"حدث خطأ أثناء إرسال بريد إلكتروني بشأن انتهاء الصلاحية إلى {toEmail} عبر Brevo SMTP: {ex.Message}");
                    }
                }
            }
            TempData["Message"] = "تم إرسال رسائل البريد الإلكتروني للمراقبين الذين ستقترب صلاحية رخصهم من الانتهاء.";
        }
        else
        {
            TempData["Message"] = "لا توجد رخص ستقترب صلاحيتها من الانتهاء لإرسال بريد إلكتروني بشأنها.";
        }

        return RedirectToAction("Notifications");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["Title"] = "ATC Controller Portal Login";
        // فقط رجّع الـ View مع الموديل، لا تعيّن Layout هون
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }



    [AllowAnonymous]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check if database is available
            if (!_db.IsDatabaseAvailable())
            {
                Console.WriteLine("❌ Database is not available during login attempt");
                ModelState.AddModelError("", "Database connection is not available. Please check your database configuration.");
                return View(model);
            }

            Console.WriteLine($"🔍 Attempting login for user: {model.Username}");
            
            if (!_db.ValidateCredentials(model.Username, model.Password, out var userId, out var role))
            {
                Console.WriteLine($"❌ Invalid credentials for user: {model.Username}");
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            Console.WriteLine($"✅ Login successful for user: {model.Username}, Role: {role}");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, model.Username),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                          new ClaimsPrincipal(identity));

            // بعد النجاح:
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            // بناءً على الرول:
            if (role == "Admin")
                return RedirectToAction("Index", "Home");
            return RedirectToAction("Profile", "Account");
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"❌ Login error: {ex.Message}");
            Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            
            ModelState.AddModelError("", $"An error occurred during login: {ex.Message}");
            return View(model);
        }
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    // استبدل أكشن Profile (GET) الحالي عندك بهذا الكود المحدث

    [Authorize]
    public IActionResult Profile()
    {
        var username = User.Identity.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        // Simply call the unified method from SqlServerDb
        var viewModel = _db.GetProfileDataByUsername(username);

        if (viewModel == null)
        {
            ViewBag.ErrorMessage = "User profile not found.";
            return View(new ProfileViewModel()!); // Return an empty page if user doesn't exist
        }

        // Send the fully populated model to the view
        return View(viewModel);
    }


    // The POST action for Profile handles the update logic smartly.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Profile(ProfileViewModel model)
    {
        // Password is optional, so we remove it from validation to allow empty submissions.
        ModelState.Remove("Password");
        // PhotoFile is also optional.
        ModelState.Remove("PhotoFile");

        if (!ModelState.IsValid)
        {
            // If validation fails, repopulate the full model to avoid errors on page reload.
            var repopulatedModel = _db.GetProfileDataByUsername(model.Username);
            return View(repopulatedModel ?? new ProfileViewModel()!);
        }

        try
        {
            // Smart Update Logic based on UserType from the form
            if (model.UserType == "Controller")
            {
                var controllerToUpdate = new ControllerUser
                {
                    ControllerId = model.UserId,
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    DateOfBirth = model.DateOfBirth,
                    MaritalStatus = model.MaritalStatus,
                    Address = model.Address,
                    EmergencyContact = model.EmergencyContact,
                    EducationLevel = model.EducationLevel,
                };
                _db.UpdateControllerProfile(controllerToUpdate);
            }
            else if (model.UserType == "Employee")
            {
                var employeeToUpdate = new Employee
                {
                    EmployeeID = model.UserId,
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    EmergencyContactPhone = model.EmergencyContact,
                    Gender = model.Gender,
                    Location = model.Location,
                };
                _db.UpdateEmployeeProfile(employeeToUpdate);
            }

            // Update password separately only if a new one was provided
            if (!string.IsNullOrEmpty(model.Password))
            {
                _db.UpdateUserPassword(model.Username, model.Password);
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while updating: " + ex.Message);
            var repopulatedModel = _db.GetProfileDataByUsername(model.Username);
            return View(repopulatedModel);
        }
    }



    [Authorize]
    public IActionResult SomeAction()
    {
        ViewBag.Database = _db; // افترض أن _db هو حقل في الـ Controller
        return View();
    }

    [Authorize]
    public IActionResult AccessDenied() => View();
}
