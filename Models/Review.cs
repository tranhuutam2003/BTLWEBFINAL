using System.ComponentModel.DataAnnotations;

namespace TestWeb.Models
{
    public class Review
    {
        [Key]
        public int ReviewID { get; set; }
        public int BookID { get; set; }
        public string? PhoneNumber { get; set; }
        [Range(1, 5, ErrorMessage = "Đánh giá phải nằm trong khoảng từ 1 đến 5.")]
        public int Rating { get; set; }  // Rating phải nằm trong khoảng 1 đến 5
        public string? Comment { get; set; }
        public DateTime ReviewDate { get; set; } = DateTime.Now;

        public Books? Book { get; set; }
        public User? User { get; set; }
    }
}