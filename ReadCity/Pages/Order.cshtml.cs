using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadCity.Models;
using ReadCity.Data;

namespace ReadCity.Pages
{
    public class OrderModel : PageModel
    {
        public bool IsEdit { get; set; } = false;
        [BindProperty] public Order Item{ get; set; }
        public List<SelectListItem> PickupPoints { get; set; } = new();
        public IActionResult OnGet(int? id)
        {
            if (HttpContext.Session.GetString("Role") != "Администратор")
                return RedirectToPage("/Orders");
            LoadList();
            if (id.HasValue)
            {
                IsEdit = true;                
                Item = Database.GetOrders().FirstOrDefault(i => i.Id == id);
                if (Item == null) return RedirectToPage("/Orders");
                return Page();
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            if (HttpContext.Session.GetString("Role") != "Администратор")
                return RedirectToPage("/Orders");
            if (Item.Id==0)
            {
                Item.OrderDate = DateTime.Now.ToString("yyyy-MM-dd");
                Item.UserId = HttpContext.Session.GetInt32("UserId").Value;
                Database.InsertOrder(Item);
                return RedirectToPage("/Orders");
            }
            else
            {                
                Database.UpdateOrder(Item);                
            }
            return RedirectToPage("/Orders");
        }
        private void LoadList()
        {
            PickupPoints = Database.GetPickupPoints()
                .Select(pp => new SelectListItem(pp.Name,pp.Id.ToString())).ToList();
        }
    }
}
