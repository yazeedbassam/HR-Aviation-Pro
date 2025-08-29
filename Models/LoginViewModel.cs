using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class LoginViewModel
    {
        [BindProperty]
        [Required]
        public string Username { get; set; }
        [BindProperty]
        [Required] 
        public string Password { get; set; }
        public string? ReturnUrl { get; set; }

        //  [Required(ErrorMessage = "الاسم مطلوب")]
        public string? Name { get; set; }

        //  [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "بريد إلكتروني غير صالح")]
        public string? Email { get; set; }

        // [Required(ErrorMessage = "الرسالة مطلوبة")]
        public string? Message { get; set; }

    }
}
