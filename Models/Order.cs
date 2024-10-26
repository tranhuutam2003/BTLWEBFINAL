using System.ComponentModel.DataAnnotations;

namespace TestWeb.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }

        public string? ShippingAddress { get; set; }

        public User? User { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
        public Payment? Payment { get; set; }
    }
}
