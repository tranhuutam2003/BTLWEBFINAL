using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TestWeb.Models;
using TestWeb.Data;
using System.Linq;
using System.Net.Mail;
using System.Net;
using Newtonsoft.Json;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace TestWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly BookContext _context;
        private const string VerificationCodeKey = "_VerificationCode";
        private const string EmailForVerification = "_EmailForVerification";

        public AccountController(BookContext context)
        {
            _context = context;
        }

        // Trang hiển thị form Reset Password
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string email)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == email);
            if (existingUser == null)
            {
                return Json(new { success = false, message = "Email chưa tồn tại trong hệ thống." });
            }

            // Gửi mã xác nhận qua email cho người dùng
            SendVerificationEmail(email);
            return Json(new { success = true, message = "Mã xác nhận đã được gửi tới email của bạn." });
        }

        // Gửi mã xác nhận qua email
        [HttpPost]
        public IActionResult SendVerificationEmail(string email)
        {
            string verificationCode = GenerateVerificationCode();
            HttpContext.Session.SetString(VerificationCodeKey, verificationCode);
            HttpContext.Session.SetString(EmailForVerification, email);
            SendEmail(email, verificationCode);

            return Json(new { success = true, message = "Mã xác nhận đã được gửi tới email của bạn." });
        }

        private string GenerateVerificationCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        // Gửi email với mã xác nhận
        private void SendEmail(string email, string code)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("tam62533@gmail.com", "dotnnpjevidbdxjr"), // Sử dụng mật khẩu ứng dụng nếu cần
                    EnableSsl = true,
                };

                smtpClient.Send("tam62533@gmail.com", email, "Mã xác nhận", $"Mã xác nhận của bạn là: {code}");
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

        // Form nhập mã xác nhận và mật khẩu mới
        public IActionResult ConfirmCode()
        {
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }
            return View();
        }

        // Xác nhận mã và cập nhật mật khẩu
        [HttpPost]
        public IActionResult ConfirmCode(string enteredCode, string newPassword, string confirmPassword)
        {
            string storedCode = HttpContext.Session.GetString(VerificationCodeKey);
            string email = HttpContext.Session.GetString(EmailForVerification);

            if (string.IsNullOrEmpty(storedCode) || storedCode != enteredCode)
            {
                ModelState.AddModelError("", "Mã xác nhận không đúng hoặc đã hết hạn.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu không khớp.");
                return View();
            }

            UpdatePassword(email, newPassword);

            // Xóa mã xác nhận và email khỏi Session sau khi sử dụng thành công
            HttpContext.Session.Remove(VerificationCodeKey);
            HttpContext.Session.Remove(EmailForVerification);

            TempData["SuccessMessage"] = "Mật khẩu đã được thay đổi thành công!";
            return RedirectToAction("Login");
        }

        // Cập nhật mật khẩu trong cơ sở dữ liệu
        private void UpdatePassword(string email, string newPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                user.Password = newPassword; // Lưu ý: Nên mã hóa mật khẩu trước khi lưu
                _context.SaveChanges();
            }
        }

        public IActionResult Login()
        {
            return PartialView();
        }

        [HttpPost]

        public IActionResult Login(User user)
        {
            // Kiểm tra user có tồn tại trong cơ sở dữ liệu không
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email && u.Password == user.Password);

            if (existingUser != null)
            {
                // Lưu thông tin phiên đăng nhập
                HttpContext.Session.SetString("Username", existingUser.Email);
                HttpContext.Session.SetInt32("Role", existingUser.Role);
                HttpContext.Session.SetString("FullName", existingUser.FullName);
                HttpContext.Session.SetString("PhoneNumber", existingUser.PhoneNumber);

                // Kiểm tra phân quyền
                if (existingUser.Role == 1) // Admin
                {
                    return RedirectToAction("Index", "Admin"); // Đưa về trang quản lý của admin
                }
                else // Khách hàng
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Message = "Invalid login attempt";
            return View();
        }

        public IActionResult Register()
        {
            return PartialView();
        }

        [HttpPost]
        public IActionResult Register(User user, string VerificationCode)
        {
            // Kiểm tra xem email đã tồn tại chưa
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại trong hệ thống.");
                return View(user);
            }

            // Kiểm tra mã xác nhận từ Session thay vì TempData
            string storedCode = HttpContext.Session.GetString(VerificationCodeKey);
            string emailForVerification = HttpContext.Session.GetString(EmailForVerification);

            // Xác minh mã xác nhận và email
            if (string.IsNullOrEmpty(storedCode) || storedCode != VerificationCode || emailForVerification != user.Email)
            {
                ModelState.AddModelError("VerificationCode", "Mã xác nhận không đúng. Vui lòng thử lại.");
                return View(user);
            }

            // Nếu tất cả hợp lệ, thêm người dùng vào cơ sở dữ liệu
            if (ModelState.IsValid)
            {
                user.Role = 0; // Đặt mặc định là khách hàng
                _context.Users.Add(user);
                _context.SaveChanges();

                // Xóa mã xác nhận và email khỏi Session sau khi sử dụng thành công
                HttpContext.Session.Remove(VerificationCodeKey);
                HttpContext.Session.Remove(EmailForVerification);

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            return View(user);
        }


        [HttpGet]
        public IActionResult EditProfile()
        {
            var userEmail = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        public IActionResult EditProfile(User updatedUser)
        {
            var userEmail = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                user.FullName = updatedUser.FullName;
                user.PhoneNumber = updatedUser.PhoneNumber;
                user.Address = updatedUser.Address;

                if (!string.IsNullOrEmpty(updatedUser.Password))
                {
                    user.Password = updatedUser.Password; // Note: In a real application, you should hash the password
                }

                _context.Update(user);
                _context.SaveChanges();

                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("PhoneNumber", user.PhoneNumber);

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Index", "Home");
            }

            return View(updatedUser);
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Username");
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}