using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ReadCity.Pages
{
    public class GuestModel : PageModel
    {
        public IActionResult OnGet()
        {
            HttpContext.Session.SetString("Role","Гость");
            HttpContext.Session.SetString("FullName", "Гость");
            return RedirectToPage("/Products");
        }
    }
}
