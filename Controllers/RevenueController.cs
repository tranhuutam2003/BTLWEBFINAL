using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestWeb.Data;
using TestWeb.Models.Authentication;

namespace TestWeb.Controllers
{
    [Authentication(1)]
    public class RevenueController : Controller
    {
        private readonly BookContext _context;

        public RevenueController(BookContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> DailyRevenue()
        {
            var dailyRevenue = await _context.Revenues
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            return View(dailyRevenue);
        }

        [HttpGet("/api/revenue/daily")]
        public async Task<IActionResult> GetDailyRevenue()
        {
            var dailyRevenue = await _context.Revenues
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            return Ok(dailyRevenue);
        }

        [HttpGet("/api/revenue/daily/{month}/{year}")]
        public async Task<IActionResult> GetDailyRevenue(int month, int year)
        {
            var dailyRevenue = await _context.Revenues
                .Where(r => r.Date.Month == month && r.Date.Year == year)
                .OrderBy(r => r.Date)
                .Select(r => new { r.Date, r.TotalSales })
                .ToListAsync();

            return Ok(dailyRevenue);
        }
        // API doanh thu theo tháng trong một năm
        [HttpGet("/api/revenue/monthly/{year}")]
        public async Task<IActionResult> GetMonthlyRevenue(int year)
        {
            var monthlyRevenue = await _context.Revenues
                .Where(r => r.Date.Year == year)
                .GroupBy(r => r.Date.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TotalSales = g.Sum(r => r.TotalSales)
                })
                .OrderBy(r => r.Month)
                .ToListAsync();

            return Ok(monthlyRevenue);
        }

        public IActionResult RevenueCharts()
        {
            return View();
        }
        [HttpGet("/api/revenue/custom")]
        public async Task<IActionResult> GetRevenueByDateRange(DateTime startDate, DateTime endDate)
        {
            var revenue = await _context.Revenues
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .GroupBy(r => r.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalSales = g.Sum(r => r.TotalSales)
                })
                .OrderBy(r => r.Date)
                .ToListAsync();

            return Ok(revenue);
        }

    }
}
