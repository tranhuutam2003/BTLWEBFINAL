using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using TestWeb.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

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
                    // Cập nhật trạng thái đơn hàng theo logic của bạn
                    if (order.Status == "Pending Confirmation")
                    {
                        order.Status = "Waiting for Pickup"; // Ví dụ chuyển sang trạng thái khác
                    }
                    else if (order.Status == "Waiting for Pickup")
                    {
                        order.Status = "Waiting for Delivery"; // Tiếp tục chuyển
                    }
                    // Cập nhật đơn hàng
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
