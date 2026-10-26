using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadCity.Models;
using ReadCity.Data;
namespace ReadCity.Pages
{
    public class OrdersModel : PageModel
    {
        public bool IsAdmin => HttpContext.Session.GetString("Role") == "Администратор";
        [BindProperty]public List<Order> Orders{ get; set; }
        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("Role") != "Администратор" &&
                HttpContext.Session.GetString("Role") != "Менеджер")
                return RedirectToPage("/Products");

            Orders = Database.GetOrders();
            return Page();
        }
    }
}
