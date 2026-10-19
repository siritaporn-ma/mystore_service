namespace MyApi.DTO.Models.ResponseModels;

// แถวในตะกร้าที่ส่งกลับให้ client พร้อมชื่อสินค้า (ไม่เปิดเผย audit fields)
public class ShoppingCartResponse
{
    public string OrderNo { get; set; } = string.Empty;

    public string ProductCode { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public int Qty { get; set; }

    public decimal Price { get; set; }
}
