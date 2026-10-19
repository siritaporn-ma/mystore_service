namespace MyApi.DTO.Models.ResponseModels;

// ข้อมูลสินค้าที่ส่งกลับให้ client (ไม่เปิดเผย audit fields)
public class ProductResponse
{
    public string ProductCode { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    // ยอดคงเหลือจากตาราง Stock; สินค้าที่ยังไม่มีแถวใน Stock จะเป็น 0
    public int Stock { get; set; }
}
