using System.ComponentModel.DataAnnotations;

namespace MyApi.DTO.Interfaces;

// รายการสินค้า 1 แถวในตะกร้า (endpoint รับเป็น List) ไม่มี Id/audit fields กัน over-posting
public class CreateCartItemRequest
{
    [Required]
    [MaxLength(50)]
    public string OrderNo { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ProductCode { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Qty { get; set; }

    [Range(0.0, 9999999999999999.99)]
    public decimal Price { get; set; }
}
