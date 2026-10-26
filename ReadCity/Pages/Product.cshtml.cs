using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReadCity.Models;
using ReadCity.Data;
using System.Drawing;
using System.IO;
namespace ReadCity.Pages
{
    public class ProductModel : PageModel
    {
        private readonly IWebHostEnvironment _env;
        public ProductModel(IWebHostEnvironment env) { _env = env; }
        
        [BindProperty] public IFormFile UploadedImage { get; set; }

        public List<SelectListItem> Suppliers { get; set; } = new();
        public List<SelectListItem> Manufacturers { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();

        [BindProperty] public Product Item { get; set; } = new();
        public bool IsEdit { get; set; } = false;

        public IActionResult OnGet(int?id)
        {
            if (HttpContext.Session.GetString("Role") != "Администратор")
                return RedirectToPage("/Products");
            LoadLists();

            if (id.HasValue)
            {
                IsEdit = true;
                var product = Database.GetProducts();
                Item = product.FirstOrDefault(p => p.Id == id);
                if (Item == null) return RedirectToPage("/Products");
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            if (HttpContext.Session.GetString("Role") != "Администратор")
                return RedirectToPage("/Products");

            if(!ModelState.IsValid)
            {
                LoadLists();
                return Page();
            }
            string oldImagePath = "";
            if(UploadedImage !=null && UploadedImage.Length>0)
            {
                if(Item.Id!=0)
                {
                    var product = Database.GetProducts().FirstOrDefault(p => p.Id == Item.Id);
                    if(product!=null)
                        oldImagePath = product.ImagePath;
                    
                }
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);

                var fileName = Guid.NewGuid() + Path.GetExtension(UploadedImage.FileName);
                var fullPath = Path.Combine(uploads, fileName);

                using (var originalImage = Image.FromStream(UploadedImage.OpenReadStream()))
                {
                    using (var resizedImage = new Bitmap(originalImage,300,200))
                    {
                        resizedImage.Save(fullPath);
                    }
                }
                
                Item.ImagePath = "/uploads/" + fileName;
                
                if(!string.IsNullOrEmpty(oldImagePath) && !oldImagePath.Contains("picture.png"))
                {
                    string path = Path.Combine(_env.WebRootPath, oldImagePath.TrimStart('/'));

                    if(System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
            }
            if(Item.Id == 0)            
                Database.InsertProduct(Item);            
            else                       
                Database.UpdateProduct(Item);
            return RedirectToPage("/Products");
        }

        private void LoadLists()
        {
            Suppliers = Database.GetSuppliers()
                .Select(s => new SelectListItem(s.Name, s.Id.ToString())).ToList();
            Manufacturers = Database.GetManufacturers()
                .Select(m => new SelectListItem(m.Name, m.Id.ToString())).ToList();
            Categories = Database.GetCategories()
                .Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
        }
    }
}
