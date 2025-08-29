using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using System;

namespace WebApplication1.Controllers
{
    public class Contact_US : Controller
    {
        [HttpPost]
        public IActionResult SendEmail(string name, string email, string message)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.office365.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("yazeedbassam@hotmail.com", "sdfsdfsdfdsfdsfsdf"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(email),
                    Subject = "رسالة جديدة من نموذج الاتصال",
                    Body = $"الاسم: {name}\nالبريد الإلكتروني: {email}\nالرسالة:\n{message}",
                    IsBodyHtml = false
                };
                mailMessage.To.Add("yazeedbassam@hotmail.com"); // غالبًا ما يكون نفس بريد hotmail الخاص بك

                smtpClient.Send(mailMessage);

                return Json(new { success = true, message = "تم إرسال رسالتك بنجاح!" });
            }
            catch (Exception ex)
            {
                // يمكنك هنا تسجيل الخطأ
                return Json(new { success = false, message = "حدث خطأ أثناء إرسال الرسالة." });
            }
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}