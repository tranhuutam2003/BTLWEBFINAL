namespace TestWeb.Models
{
    public class Revenue
    {
        public int RevenueID { get; set; }
        public DateTime Date { get; set; }  // Ngày ghi nhận
        public decimal TotalSales { get; set; }  // Tổng doanh thu
        public int TotalOrders { get; set; }  // Tổng số đơn hàng trong ngày
        public decimal TotalProfit { get; set; }  // Tổng lợi nhuận, nếu có tính toán chi phí
    }
}
