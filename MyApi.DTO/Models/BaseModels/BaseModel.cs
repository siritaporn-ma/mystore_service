using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.DTO.Models.BaseModels;

public class BaseModel
{
    // ค่า default ของระบบ ใช้เมื่อยังไม่มีผู้ใช้ปัจจุบัน (ยังไม่มี auth) ภายหลังให้ชั้น Service/DAL ตั้งค่าจากผู้ใช้จริง
    public const string DefaultCreatedBy = "system";

    [Required]
    [Column(TypeName = "nvarchar(250)")]
    public string CreatedBy { get; set; } = DefaultCreatedBy;

    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
