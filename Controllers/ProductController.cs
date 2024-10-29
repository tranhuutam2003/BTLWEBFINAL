using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestWeb.Data;
using TestWeb.ViewModel;
using X.PagedList;

namespace TestWeb.Controllers
{
    public class ProductController : Controller
    {
        private readonly BookContext _context;

        public ProductController(BookContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Search(string query, int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;
            var lstsanpham = _context.Books.AsNoTracking()
                .Where(x => x.Title.Contains(query) || x.Author.Contains(query) || x.Category.CategoryName.Contains(query))
                .OrderBy(x => x.Title);
            var lst = lstsanpham.ToPagedList(pageNumber, pageSize);
            ViewBag.Query = query;
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductList", lst);
            }
            return View(lst);
        }

        public IActionResult SanPhamTheoLoai(int maloai, int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;
            var lstsanpham = _context.Books.AsNoTracking().Where(x => x.CategoryID == maloai).OrderBy(x => x.Title);
            var lst = lstsanpham.ToPagedList(pageNumber, pageSize);
            ViewBag.maloai = maloai;
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductList", lst);
            }
            return View(lst);
        }

        public IActionResult BookDetail(int mabook)
        {
            var book = _context.Books.SingleOrDefault(x => x.BookID == mabook);
            var detailViewModel = new DetailViewModel { books = book };
            return View(detailViewModel);
        }

        public IActionResult BookList(int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;
            var lstsanpham = _context.Books.AsNoTracking().OrderBy(x => x.Title);
            var lst = lstsanpham.ToPagedList(pageNumber, pageSize);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductList", lst);
            }
            return View(lst);
        }
    }
}