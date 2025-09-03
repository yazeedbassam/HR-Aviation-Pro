using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class Contact_US : Controller
    {
        private readonly IEmailService _emailService;

        public Contact_US(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(string name, string email, string message)
        {
            try
            {
                var success = await _emailService.SendContactFormAsync(name, email, message);
                
                if (success)
                {
                    return Json(new { success = true, message = "تم إرسال رسالتك بنجاح!" });
                }
                else
                {
                    return Json(new { success = false, message = "حدث خطأ أثناء إرسال الرسالة." });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء إرسال الرسالة." });
            }
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}