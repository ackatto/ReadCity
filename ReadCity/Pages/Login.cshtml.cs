using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadCity.Data;
using System.ComponentModel.DataAnnotations;
namespace ReadCity.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Введите логин")]
        public string Login { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Введите пароль")]
        public string Password { get; set; }
        public IActionResult OnPost()
        {    
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = Database.GetUser(Login, Password);
            if(user==null)
            {
                ModelState.AddModelError("", "Неверный логин или пароль");
                return Page();
            }
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Role", user.Role);
            return RedirectToPage("/Products");
        }
    }
}
