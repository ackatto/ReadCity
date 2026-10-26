namespace ReadCity.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderDate { get; set; }
        public string DeliveryDate { get; set; }
        public int PickupPointId { get; set; }
        public string PickupPointName{ get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public int Code { get; set; }
        public string Status { get; set; }
    }
}
