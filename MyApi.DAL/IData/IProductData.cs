using MyApi.DTO.Models.ResponseModels;

namespace MyApi.DAL.IData;

public interface IProductData
{
    // skip = จำนวนรายการที่ข้าม, take = จำนวนรายการที่ดึง; คืนรายการของหน้านั้น (พร้อมยอด Stock) พร้อมจำนวนทั้งหมด
    Task<(IReadOnlyList<ProductResponse> Items, int TotalCount)> GetPagedAsync(int skip, int take);
}
