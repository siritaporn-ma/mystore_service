using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.DTO.Models.BaseModels;

// BaseModel + ข้อมูลการแก้ไข (CreatedBy/CreatedDate สืบทอดมาจาก BaseModel)
public class FullBaseModel : BaseModel
{
    // null = ยังไม่เคยถูกแก้ไข ชั้น Service/DAL ต้องตั้งค่าจากผู้ใช้ปัจจุบันตอน update
    [Column(TypeName = "nvarchar(250)")]
    public string? UpdatedBy { get; set; }

    // null = ยังไม่เคยถูกแก้ไข ควรเก็บเป็นเวลา UTC เหมือน CreatedDate
    public DateTime? UpdatedDate { get; set; }
}
