using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadCity.Models;
using ReadCity.Data;
namespace ReadCity.Pages
{
    public class ProductsModel : PageModel
    {
        public string Search { get; set; } = "";
        public string SortColumn { get; set; } = "p.Price";
        public string SortDirection { get; set; } = "ASC";
        public string DiscountRange { get; set; } = "";
        public int currentPage{ get; set; }
        public bool IsAdmin => HttpContext.Session.GetString("Role") == "Администратор";
        public bool IsManager => IsAdmin || HttpContext.Session.GetString("Role") == "Менеджер";
        public List<Product> Products { get; set; } = new();
        public IActionResult OnGet(string search, string discountRange, string sortColumn, string sortDirection,int pageNumber)
        {
            if (HttpContext.Session.GetString("Role") == null)
                return RedirectToPage("/Login");
            if(IsManager)
            {
                currentPage = pageNumber < 1 ? 1 : pageNumber;
                int offset = (pageNumber - 1) * pageNumber; ;
                Search = search ?? "";
                SortColumn = sortColumn ?? "";
                SortDirection = sortDirection ?? ""; 
                DiscountRange = discountRange ?? "";

                SortColumn = string.IsNullOrEmpty(SortColumn) ? "p.Price" : SortColumn;
                SortDirection = string.IsNullOrEmpty(SortDirection) ? "ASC" : SortDirection;

                Products = Database.GetProducts(Search, DiscountRange, SortColumn, SortDirection, offset);
            }
            else
            {
                // Гостям и клиентам сортируем по ID (естественный порядок)
                Products = Database.GetProducts("", "", "p.Id", "ASC");
            }           
            return Page();
        }
    }
}
