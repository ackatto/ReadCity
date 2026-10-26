using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadCity.Models;
using ReadCity.Data;
namespace ReadCity.Pages
{
    public class OrderDeleteModel : PageModel
    {
        public int OrderId { get; set; }
        public int OrderArticle{ get; set; }
        public string Error { get; set; }

        public IActionResult OnGet(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Администратор")
                return RedirectToPage("/Orders");

            OrderId = id;
            var Order = Database.GetOrders().FirstOrDefault(o => o.Id == id);
            OrderArticle = Order.Id;
            return Page();
        }

        public IActionResult OnPost(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Администратор")
                return RedirectToPage("/Orders");
            try
            {
                Database.DeleteOrder(id);
                return RedirectToPage("/Orders");
            }
            catch(Exception ex)
            {
                OrderId = id;
                Error = ex.Message;
                var Order = Database.GetOrders().FirstOrDefault(o => o.Id == id);
                if(Order==null) return RedirectToPage("/Orders");
                OrderArticle = Order.Id;
                return Page();
            }
        }
    }
}
