using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestWeb.Models
{
    public class Books
    {
        [Key]
        public int BookID { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống.")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Tác giả không được để trống.")]
        public string? Author { get; set; }

        [Required(ErrorMessage = "Nhà xuất bản không được để trống.")]
        public string? Publisher { get; set; }

        [Required(ErrorMessage = "Ngày xuất bản không được để trống.")]
        public DateTime PublishedDate { get; set; }

        public int CategoryID { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0.")]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng trong kho phải lớn hơn hoặc bằng 0.")]
        public int StockQuantity { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
        public string? Description { get; set; }

        [NotMapped] // Không lưu thuộc tính này vào cơ sở dữ liệu
        public IFormFile? ImageFile { get; set; }
        [Required(ErrorMessage = "Ảnh không được để trống.")]
        public string? ImageURL { get; set; }

        public Category? Category { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}