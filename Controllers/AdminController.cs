using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestWeb.Data;
using TestWeb.Models;
using TestWeb.Models.Authentication;
using X.PagedList;

namespace TestWeb.Controllers
{
    [Authentication(1)]
    public class AdminController : Controller
    {

        private readonly BookContext _context;

        // Sử dụng Dependency Injection để nhận BookContext
        public AdminController(BookContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách sản phẩm
        public IActionResult Index()
        {
            var listBook = _context.Books.ToList();
            return View(listBook);
        }

        // Hiển thị toàn bộ danh sách sách
        public IActionResult AllBooks(int? page)
        {
            int pageSize = 10; // Số lượng sách hiển thị trên mỗi trang
            int pageNumber = page ?? 1; // Nếu page là null thì sẽ là trang 1

            var books = _context.Books.Include(b => b.Category).ToPagedList(pageNumber, pageSize);
            return View(books);
        }

        public IActionResult Categories(int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var categories = _context.Categories.ToPagedList(pageNumber, pageSize);
            return View(categories);
        }

        [HttpGet]
        public IActionResult AddBook()
        {
            ViewBag.CategoryList = new SelectList(_context.Categories, "CategoryID", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBook(Books book)
        {
            if (ModelState.IsValid)
            {
                if (book.ImageFile != null)
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "products");

                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(imagePath))
                    {
                        Directory.CreateDirectory(imagePath);
                    }

                    // Tạo tên file duy nhất
                    var fileName = Path.GetFileNameWithoutExtension(book.ImageFile.FileName);
                    var extension = Path.GetExtension(book.ImageFile.FileName);
                    book.ImageURL = fileName + DateTime.Now.ToString("yymmssfff") + extension;

                    var filePath = Path.Combine(imagePath, book.ImageURL);

                    try
                    {
                        // Lưu ảnh vào thư mục
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await book.ImageFile.CopyToAsync(fileStream);
                        }
                    }
                    catch (Exception ex)
                    {
                        //TempData["ErrorMessage"] = $"Error saving the image: {ex.Message}";
                        return RedirectToAction("AddBook"); // Hoặc trang bạn muốn quay lại
                    }
                }
                else
                {
                    //TempData["ErrorMessage"] = "No image file was uploaded.";
                    return RedirectToAction("AddBook"); // Hoặc trang bạn muốn quay lại
                }

                try
                {
                    _context.Books.Add(book);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    //TempData["ErrorMessage"] = $"Error saving the book: {ex.Message}";
                    return RedirectToAction("AddBook"); // Hoặc trang bạn muốn quay lại
                }
            }

            ViewBag.CategoryList = new SelectList(_context.Categories, "CategoryID", "CategoryName");
            return View(book);
        }

        [HttpGet]
        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        // Sửa sản phẩm - Hiển thị form sửa
        [HttpGet]
        public IActionResult EditBook(int id)
        {
            var book = _context.Books.Find(id); // Tìm sách theo id
            if (book == null)
            {
                return NotFound(); // Nếu không tìm thấy sách, trả về NotFound
            }
            ViewBag.CategoryList = new SelectList(_context.Categories, "CategoryID", "CategoryName");
            return View(book); // Hiển thị form chỉnh sửa với thông tin sách
        }

        // Sửa sản phẩm - Xử lý khi người dùng submit form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBook(Books book)
        {
            if (ModelState.IsValid)
            {
                if (book.ImageFile != null)
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "products");

                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(imagePath))
                    {
                        Directory.CreateDirectory(imagePath);
                    }

                    // Tạo tên file duy nhất
                    var fileName = Path.GetFileNameWithoutExtension(book.ImageFile.FileName);
                    var extension = Path.GetExtension(book.ImageFile.FileName);
                    book.ImageURL = fileName + DateTime.Now.ToString("yymmssfff") + extension;

                    var filePath = Path.Combine(imagePath, book.ImageURL);

                    try
                    {
                        // Lưu ảnh vào thư mục
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await book.ImageFile.CopyToAsync(fileStream);
                        }
                    }
                    catch (Exception ex)
                    {
                        //TempData["ErrorMessage"] = $"Error saving the image: {ex.Message}";
                        return RedirectToAction("EditBook", new { id = book.BookID }); // Hoặc trang bạn muốn quay lại
                    }
                }

                // Cập nhật thông tin sách
                _context.Books.Update(book); // Cập nhật thông tin sách
                await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
                return RedirectToAction("Index");
            }

            ViewBag.CategoryList = new SelectList(_context.Categories, "CategoryID", "CategoryName");
            return View(book);  // Nếu không hợp lệ, hiển thị lại form với dữ liệu hiện tại
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category category)
        {
            if (id != category.CategoryID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        // Xóa sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteBook(int id)
        {
            var book = _context.Books.Find(id);
            if (book == null)
            {
                return NotFound();
            }

            // Xóa sách khỏi DbContext
            _context.Books.Remove(book);
            _context.SaveChanges();

            return RedirectToAction("AllBooks");
        }

        [HttpPost, ActionName("DeleteCategory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Categories));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryID == id);
        }

        [HttpGet]
        public IActionResult SearchBooks(string query, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var searchResults = _context.Books
                .Include(b => b.Category)
                .AsNoTracking()
                .Where(x => x.Title.Contains(query) ||
                            x.Author.Contains(query) ||
                            x.Publisher.Contains(query) ||
                            x.Category.CategoryName.Contains(query))
                .OrderBy(x => x.Title);

            var pagedResults = searchResults.ToPagedList(pageNumber, pageSize);

            ViewBag.Query = query;
            return View(pagedResults);
        }

        [HttpGet]
        public IActionResult SearchCategories(string query, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var searchResults = _context.Categories
                .AsNoTracking()
                .Where(c => c.CategoryName.Contains(query))
                .OrderBy(c => c.CategoryName);

            var pagedResults = searchResults.ToPagedList(pageNumber, pageSize);

            ViewBag.Query = query;
            return View(pagedResults);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Username");
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Dashboard()
        {
            //Trước khi đó phải kiểm tra xem ở bảng database  Order có TotalAmount >0 trong trạng thái Status ="Complete"
            var totalRevenue = _context.Orders
                .Where(o => o.Status == "Complete")
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            // Debug log
            Console.WriteLine($"Total Revenue: {totalRevenue}");

            ViewBag.TotalRevenue = totalRevenue;

            return View();
        }

    }
}
