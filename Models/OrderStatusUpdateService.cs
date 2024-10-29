using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TestWeb.Data;
using TestWeb.Models;

public class OrderStatusUpdateService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<OrderHub> _hubContext;

    public OrderStatusUpdateService(IServiceScopeFactory scopeFactory, IHubContext<OrderHub> hubContext)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _ = ExecuteAsync(cancellationToken); // Bắt đầu thực thi
    }

    private async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<BookContext>();

                // Cập nhật trạng thái đơn hàng
                var orders = _context.Orders
                    .Where(o => o.Status != "Delivered" && o.Status != "Cancelled")
                    .ToList();

                foreach (var order in orders)
                {
                    // Cập nhật trạng thái đơn hàng
                    if (order.Status == "Pending Confirmation")
                    {
                        order.Status = "Waiting for Pickup";
                    }
                    else if (order.Status == "Waiting for Pickup")
                    {
                        order.Status = "Waiting for Delivery";
                    }
                    else if (order.Status == "Waiting for Delivery")
                    {
                        order.Status = "Delivered";

                        // Ghi nhận doanh thu khi đơn hàng hoàn tất
                        var revenue = await _context.Revenues
                            .FirstOrDefaultAsync(r => r.Date == DateTime.Today);

                        if (revenue == null)
                        {
                            revenue = new Revenue
                            {
                                Date = DateTime.Today,
                                TotalSales = order.TotalAmount,
                                TotalOrders = 1,
                                TotalProfit = order.TotalAmount * 0.1m // Giả sử lợi nhuận là 10%
                            };
                            _context.Revenues.Add(revenue);
                        }
                        else
                        {
                            revenue.TotalSales += order.TotalAmount;
                            revenue.TotalOrders += 1;
                            revenue.TotalProfit += order.TotalAmount * 0.1m;
                        }
                    }
                    _context.Orders.Update(order);
                }

                await _context.SaveChangesAsync();

                // Gửi thông báo đến client thông qua SignalR
                await _hubContext.Clients.All.SendAsync("OrderStatusUpdated");
            }

            await Task.Delay(10000, stoppingToken); // Delay 10 giây
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
