namespace MyApi.DTO.Interfaces;

// พารามิเตอร์แบ่งหน้าของรายการ order (ค่า page/pageSize ที่ไม่ถูกต้องจะถูกปรับที่ชั้น Service)
public class GetListOrderRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
