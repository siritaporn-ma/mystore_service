using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyApi.DTO.Models.BaseModels;

namespace MyApi.DTO.Models.MainModels;

// Primary key เป็น composite (OrderNo, ProductCode) กำหนดใน ApplicationDBContext.OnModelCreating (HasKey)
[Table("ShoppingCart")]
public class ShoppingCart : FullBaseModel
{
    [MaxLength(50)]
    [Column(TypeName = "nvarchar(50)")]
    public string OrderNo { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column(TypeName = "nvarchar(50)")]
    public string ProductCode { get; set; } = string.Empty;

    public int Qty { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
}
