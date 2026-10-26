using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadCity.Data;
using System.Drawing;
namespace ReadCity.Pages
{
    public class DeleteModel : PageModel
    {
        private readonly IWebHostEnvironment _env;
        public DeleteModel(IWebHostEnvironment env){ _env = env; }
        public int ProductId{ get; set; }
        public string BookName { get; set; }
        public string Error { get; set; } 

        public IActionResult OnGet(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Администратор")
                return RedirectToPage("/Products");

            ProductId = id;
            var product = Database.GetProducts().FirstOrDefault(p => p.Id == id);
            if (product == null)
                return RedirectToPage("/Products");
            BookName = product.BookName;
            return Page();
        }

        public IActionResult OnPost(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Администратор")
                return RedirectToPage("/Products");

            var product = Database.GetProducts().FirstOrDefault(p => p.Id == id);
            if (product == null) return RedirectToPage("/Products");
            var currentImage = product.ImagePath;            

            try
            {
                Database.DeleteProduct(id);
                if (!string.IsNullOrWhiteSpace(currentImage) && !currentImage.Contains("picture.png"))
                {
                    var path = Path.Combine(_env.WebRootPath, currentImage.Trim('/'));
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
                return RedirectToPage("/Products");
            }
            catch(InvalidOperationException ex)
            {
                ProductId = id;
                Error = ex.Message;
                BookName = Database.GetProducts().FirstOrDefault(p => p.Id == id) ?.BookName ??"";
                return Page();
            }
        }
    }
}
