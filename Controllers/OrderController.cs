using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using TestWeb.Data;
using TestWeb.Models;
using TestWeb.Models.Authentication;

namespace TestWeb.Controllers
{
    public class OrderController : Controller
    {
        private readonly BookContext _context;
        private readonly IHubContext<OrderHub> _hubContext; // Replace 'YourHub' with your actual hub class name

        private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(10);
        public OrderController(BookContext context, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Hiển thị giỏ hàng và nhập địa chỉ giao hàng
        public IActionResult Checkout()
        {
            var userPhoneNumber = HttpContext.Session.GetString("PhoneNumber");
            if (string.IsNullOrEmpty(userPhoneNumber))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy giỏ hàng của người dùng hiện tại
            var cart = _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Book)
                .FirstOrDefault(c => c.PhoneNumber == userPhoneNumber);

            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn hiện đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            // Tạo Order model với các thông tin cần thiết
            var order = new Order
            {
                OrderDetails = cart.Items.Select(item => new OrderDetail
                {
                    BookID = item.Book?.BookID ?? 0,
                    Quantity = item.Quantity,
                    Price = item.Book?.Price ?? 0
                }).ToList(),
                TotalAmount = cart.TotalAmount
            };

            return View(order);
        }

        // Đặt hàng và gửi email xác nhận
        //[HttpPost]
        //public ActionResult PlaceOrder(Order order)
        //{
        //    var userPhoneNumber = HttpContext.Session.GetString("PhoneNumber");
        //    if (string.IsNullOrEmpty(userPhoneNumber))
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    // Lấy giỏ hàng hiện tại
        //    var cart = _context.Carts
        //        .Include(c => c.Items)
        //        .ThenInclude(i => i.Book)
        //        .FirstOrDefault(c => c.PhoneNumber == userPhoneNumber);

        //    if (cart == null || !cart.Items.Any())
        //    {
        //        TempData["Error"] = "Giỏ hàng của bạn hiện đang trống.";
        //        return RedirectToAction("Index", "Cart");
        //    }

        //    // Tạo đơn hàng mới
        //    order.PhoneNumber = userPhoneNumber;
        //    order.OrderDate = DateTime.Now;
        //    order.Status = "Pending Confirmation";

        //    _context.Orders.Add(order);
        //    _context.SaveChanges();

        //    // Thêm chi tiết đơn hàng từ các mục trong giỏ hàng
        //    foreach (var item in cart.Items)
        //    {
        //        var orderDetail = new OrderDetail
        //        {
        //            OrderID = order.OrderID,
        //            BookID = item.Book?.BookID ?? 0,
        //            Quantity = item.Quantity,
        //            Price = item.Book?.Price ?? 0
        //        };
        //        _context.OrderDetails.Add(orderDetail);
        //    }

        //    _context.SaveChanges();

        //    // Xóa giỏ hàng sau khi đặt hàng thành công
        //    _context.CartItems.RemoveRange(cart.Items);
        //    _context.SaveChanges();

        //    TempData["Success"] = "Đặt hàng thành công!";
        //    SendOrderConfirmationEmail(userPhoneNumber, order.OrderID);

        //    return RedirectToAction("OrderConfirmation", new { orderId = order.OrderID });
        //}
        [HttpPost]
        public ActionResult PlaceOrder(Order order)//update lại
        {
            var userPhoneNumber = HttpContext.Session.GetString("PhoneNumber");
            if (string.IsNullOrEmpty(userPhoneNumber))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy giỏ hàng hiện tại
            var cart = _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Book)
                .FirstOrDefault(c => c.PhoneNumber == userPhoneNumber);

            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn hiện đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            // Tạo đơn hàng mới
            order.PhoneNumber = userPhoneNumber;
            order.OrderDate = DateTime.Now;
            order.Status = "Pending Confirmation";

            // Tính toán tổng số tiền từ giỏ hàng
            order.TotalAmount = cart.TotalAmount; // Hoặc tính toán lại từ OrderDetail nếu cần

            _context.Orders.Add(order);
            _context.SaveChanges(); // Lưu đơn hàng đầu tiên để có OrderID

            // Thêm chi tiết đơn hàng từ các mục trong giỏ hàng
            foreach (var item in cart.Items)
            {
                var orderDetail = new OrderDetail
                {
                    OrderID = order.OrderID,
                    BookID = item.Book?.BookID ?? 0,
                    Quantity = item.Quantity,
                    Price = item.Book?.Price ?? 0
                };
                _context.OrderDetails.Add(orderDetail);
            }

            _context.SaveChanges(); // Lưu chi tiết đơn hàng

            // Xóa giỏ hàng sau khi đặt hàng thành công
            _context.CartItems.RemoveRange(cart.Items);
            _context.SaveChanges();

            TempData["Success"] = "Đặt hàng thành công!";
            SendOrderConfirmationEmail(userPhoneNumber, order.OrderID);

            return RedirectToAction("OrderConfirmation", new { orderId = order.OrderID });
        }


        public IActionResult TrackOrder()
        {
            var userPhoneNumber = HttpContext.Session.GetString("PhoneNumber");
            if (string.IsNullOrEmpty(userPhoneNumber))
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .Where(o => o.PhoneNumber == userPhoneNumber)
                .ToList();

            return View(orders);
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            var order = _context.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = status; // Cập nhật trạng thái
                _context.SaveChanges();

                // Gửi email thông báo cho người dùng
                var user = _context.Users.FirstOrDefault(u => u.PhoneNumber == order.PhoneNumber);
                if (user != null)
                {
                    SendOrderConfirmationEmail(order.PhoneNumber, order.OrderID);
                }

                TempData["Success"] = "Trạng thái đơn hàng đã được cập nhật.";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
            }

            return RedirectToAction("TrackOrder");
        }

        [HttpPost]
        public IActionResult CancelOrder(int orderId)
        {
            var order = _context.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = "Cancelled"; // Cập nhật trạng thái
                _context.SaveChanges();

                TempData["Success"] = "Đơn hàng đã được hủy.";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
            }

            return RedirectToAction("TrackOrder");
        }

        // Trang xác nhận đơn hàng
        public ActionResult OrderConfirmation(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .FirstOrDefault(o => o.OrderID == orderId);
            return View(order);
        }

        // Gửi email xác nhận đơn hàng
        private void SendOrderConfirmationEmail(string phoneNumber, int orderId)
        {
            var user = _context.Users.FirstOrDefault(u => u.PhoneNumber == phoneNumber);
            if (user == null) return;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("tam62533@gmail.com", "dotnnpjevidbdxjr"),
                EnableSsl = true,
            };

            string subject = "Xác nhận đơn hàng";
            string body = $"Cảm ơn bạn đã đặt hàng! Mã đơn hàng của bạn là: {orderId}";

            try
            {
                smtpClient.Send("tam62533@gmail.com", user.Email, subject, body);
            }
            catch (SmtpException smtpEx)
            {
                ViewBag.Message = $"Lỗi SMTP: {smtpEx.Message}";
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Có lỗi xảy ra khi gửi email: {ex.Message}";
            }
        }

        public async Task<IActionResult> UpdateOrderStatuses()
        {
            var orders = await _context.Orders
                .Where(o => o.Status != "Delivered" && o.Status != "Cancelled") // Kiểm tra trạng thái
                .ToListAsync();

            foreach (var order in orders)
            {
                // Giả sử bạn có một logic để xác định trạng thái mới (Ví dụ: Chuyển từ "Pending Confirmation" thành "Waiting for Pickup")
                if (order.Status == "Pending Confirmation")
                {
                    order.Status = "Waiting for Pickup";
                }
                else if (order.Status == "Waiting for Pickup")
                {
                    order.Status = "Waiting for Delivery";// Hoặc trạng thái tương ứng
                }
                else if (order.Status == "Waiting for Delivery")
                {
                    order.Status = "Complete"; // Hoặc trạng thái tương ứng
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.Orders.Update(order);
            }

            await _context.SaveChangesAsync();

            // Gửi thông báo đến client thông qua SignalR
            await _hubContext.Clients.All.SendAsync("OrderStatusUpdated");

            return Ok(); // Trả về trạng thái thành công
        }
        [Authentication(1)]
        public IActionResult AdminViewOrders()
        {
            var orders = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .ToList();
            // Tính toán lại TotalAmount cho mỗi đơn hàng nếu chưa có giá trị chính xác
            foreach (var order in orders)
            {
                order.TotalAmount = order.OrderDetails.Sum(od => od.Price * od.Quantity);
            }

            return View(orders); // Trả về view hiển thị danh sách đơn hàng
        }


    }
}
