using MyApi.DTO.Interfaces;
using MyApi.DTO.Models.ResponseModels;

namespace MyApi.BSL.IService;

public interface IShoppingCartService
{
    // สร้างแถวตะกร้าทั้งหมดและตัดสต็อกตาม Qty ในครั้งเดียว (atomic: ตะกร้า + สต็อกสำเร็จพร้อมกันหรือไม่สำเร็จเลย)
    // แล้วคืนแถวทั้งหมดของ order ในคำขอพร้อมชื่อสินค้า
    // ข้อมูลไม่ถูกต้อง / ไม่พบสินค้า / ไม่มีสต็อก / สต็อกไม่พอ -> BadRequestException, ซ้ำกับที่มีอยู่ -> ConflictException
    Task<IReadOnlyList<ShoppingCartResponse>> CreateCartAsync(List<CreateCartItemRequest>? request);

    // คืนรายการตะกร้าพร้อมชื่อสินค้าแบบแบ่งหน้า; page < 1 -> 1, pageSize ถูกจำกัดที่ 1..100
    Task<PagedResult<ShoppingCartResponse>> GetListOrderAsync(GetListOrderRequest request);
}
