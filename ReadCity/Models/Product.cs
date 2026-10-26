namespace ReadCity.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Article { get; set; }
        public string BookName { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int ManufacturerId { get; set; }
        public string ManufacturerName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal Discount { get; set; }
        public int StockQuantity { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
    }
}
