using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestWeb.Data;
using TestWeb.Models;
using System.Globalization;
using TestWeb.Models.Authentication;

namespace TestWeb.Controllers
{
    [Authentication(0)]
    public class CartController : Controller
    {
        private readonly BookContext _context;

        public CartController(BookContext context)
        {
            _context = context;
        }

        // Kiểm tra nếu người dùng đã đăng nhập
        private bool IsUserLoggedIn()
        {
            return HttpContext.Session.GetString("Username") != null;
        }

        public async Task<IActionResult> Index()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Account"); // Chuyển hướng đến trang đăng nhập
            }

            var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

            return View(cart ?? new Cart { PhoneNumber = phoneNumber });
        }

        public async Task<IActionResult> AddToCart(int bookId)
        {
            if (!IsUserLoggedIn())
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thêm vào giỏ hàng." });
            }

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại." });
            }

            var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
            var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

            if (cart == null)
            {
                cart = new Cart { PhoneNumber = phoneNumber };
                _context.Carts.Add(cart);
            }

            cart.AddItem(book, 1);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã thêm vào giỏ hàng!" });
        }

        [HttpPost]
        public async Task<IActionResult> IncreaseQuantity(int bookId)
        {
            if (!IsUserLoggedIn())
            {
                return Json(new { success = false, redirectUrl = Url.Action("Login", "Account") });
            }

            var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

            if (cart != null)
            {
                var book = await _context.Books.FindAsync(bookId);
                if (book != null)
                {
                    cart.AddItem(book, 1);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, quantity = cart.Items.First(i => i.Book.BookID == bookId).Quantity, totalAmount = GetCartTotalAmount(cart) });
                }
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> DecreaseQuantity(int bookId)
        {
            if (!IsUserLoggedIn())
            {
                return Json(new { success = false, redirectUrl = Url.Action("Login", "Account") });
            }

            var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

            if (cart != null)
            {
                var item = cart.Items.FirstOrDefault(i => i.Book.BookID == bookId);
                if (item != null)
                {
                    if (item.Quantity > 1)
                    {
                        item.Quantity--;
                    }
                    else
                    {
                        cart.RemoveItem(bookId);
                    }
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, quantity = item?.Quantity ?? 0, totalAmount = GetCartTotalAmount(cart).ToString("C", new CultureInfo("vi-VN")) });
                }
            }

            return Json(new { success = false });
        }

        private decimal GetCartTotalAmount(Cart cart)
        {
            return cart?.Items.Sum(i => i.Quantity * i.Book.Price) ?? 0;
        }

        public async Task<IActionResult> RemoveFromCart(int bookId)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

            if (cart != null)
            {
                cart.RemoveItem(bookId);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalAmount()
        {
            var phoneNumber = HttpContext.Session.GetString("PhoneNumber");
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

            var totalAmount = cart?.Items.Sum(i => i.Quantity * i.Book.Price) ?? 0;
            // Sửa lại dòng này để chỉ sử dụng một tham số
            return Json(new { totalAmount = totalAmount.ToString("C", new CultureInfo("vi-VN")) });
        }
    }
}