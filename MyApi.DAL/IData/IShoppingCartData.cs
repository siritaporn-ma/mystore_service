using MyApi.DTO.Models.MainModels;
using MyApi.DTO.Models.ResponseModels;

namespace MyApi.DAL.IData;

public interface IShoppingCartData
{
    // คืน ProductCode ที่มีอยู่จริงในตาราง Products (query เดียวสำหรับทุกรหัส)
    Task<IReadOnlyList<string>> GetExistingProductCodesAsync(IReadOnlyCollection<string> productCodes);

    // คืนคู่ (OrderNo, ProductCode) ที่มีอยู่แล้วใน ShoppingCarts (query เดียวสำหรับทุกคู่)
    Task<IReadOnlyList<(string OrderNo, string ProductCode)>> GetExistingKeysAsync(IReadOnlyCollection<(string OrderNo, string ProductCode)> keys);

    // เพิ่มแถวตะกร้าทั้งหมดและตัดสต็อกตาม Qty ใน transaction เดียว (all-or-nothing)
    // แต่ละรายการตัดสต็อกด้วย update แบบมีเงื่อนไข Balance >= Qty กัน oversell
    // คืน true เมื่อสำเร็จและ commit; คืน false เมื่อมีรายการใดสต็อกไม่พอ (rollback ทั้งหมด แถวตะกร้าไม่ค้างอยู่)
    Task<bool> AddRangeAndDeductStockAsync(IReadOnlyCollection<ShoppingCart> items);

    // ดึงแถวในตะกร้าของ order ที่ระบุ join กับ Products เพื่อเอาชื่อสินค้า เรียงตาม OrderNo แล้ว ProductCode
    Task<IReadOnlyList<ShoppingCartResponse>> GetByOrderNosAsync(IReadOnlyCollection<string> orderNos);

    // skip = จำนวนรายการที่ข้าม, take = จำนวนรายการที่ดึง; คืนแถวในตะกร้าของหน้านั้น (join กับ Products) พร้อมจำนวนทั้งหมด
    Task<(IReadOnlyList<ShoppingCartResponse> Items, int TotalCount)> GetListPagedAsync(int skip, int take);

    // คืนยอดคงเหลือ (ProductCode, Balance) ในตาราง Stock ของรหัสที่ระบุ (query เดียว)
    Task<IReadOnlyList<(string ProductCode, int Balance)>> GetStockBalancesAsync(IReadOnlyCollection<string> productCodes);
}
