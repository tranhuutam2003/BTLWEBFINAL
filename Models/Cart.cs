
namespace TestWeb.Models
{
    public class Cart
    {
        public int CartID { get; set; }
        public string? PhoneNumber { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public User? User { get; set; }

        public void AddItem(Books book, int quantity)
        {
            var item = Items.FirstOrDefault(i => i.Book != null && i.Book.BookID == book.BookID);
            if (item == null)
            {
                Items.Add(new CartItem { Book = book, Quantity = quantity });
            }
            else
            {
                item.Quantity += quantity;
            }
        }

        public void RemoveItem(int bookId)
        {
            Items.RemoveAll(i => i.Book != null && i.Book.BookID == bookId);
        }

        public decimal TotalAmount => Items.Sum(i =>
            (i.Book != null && i.Book.Price.HasValue ? i.Book.Price.Value : 0) * i.Quantity);
    }
}
