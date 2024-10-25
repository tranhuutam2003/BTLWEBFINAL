using System.ComponentModel.DataAnnotations;

namespace TestWeb.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }
        [Required(ErrorMessage = "Tên danh mục không được để trống.")]
        public string? CategoryName { get; set; }

        public ICollection<Books>? Books { get; set; }
    }
}
