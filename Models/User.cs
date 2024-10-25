using System.ComponentModel.DataAnnotations;

namespace TestWeb.Models
{
    public class User
    {
        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Email phải có đuôi @gmail.com.")]
        public string? Email { get; set; }

        [Key]
        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [StringLength(10, ErrorMessage = "Số điện thoại phải có 10 ký tự.", MinimumLength = 10)]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và có 10 số.")]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
        public string? Password { get; set; }

        public int Role { get; set; }  // 0: Khách hàng, 1: Admin

        public ICollection<Order>? Orders { get; set; }
        public Cart? Cart { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}