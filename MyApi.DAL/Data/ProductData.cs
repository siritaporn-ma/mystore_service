using Microsoft.EntityFrameworkCore;
using MyApi.DAL.Connections;
using MyApi.DAL.IData;
using MyApi.DTO.Models.ResponseModels;

namespace MyApi.DAL.Data;

public class ProductData : IProductData
{
    private readonly ApplicationDBContext _context;

    public ProductData(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<ProductResponse> Items, int TotalCount)> GetPagedAsync(int skip, int take)
    {
        var totalCount = await _context.Products.CountAsync();

        // Stock ไม่มี FK กับ Product จึง left join ด้วย ProductCode; ไม่มีแถว Stock -> 0
        var query =
            from p in _context.Products.AsNoTracking()
            join s in _context.Stocks.AsNoTracking() on p.ProductCode equals s.ProductCode into stocks
            from s in stocks.DefaultIfEmpty()
            select new ProductResponse
            {
                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                Price = p.Price,
                Stock = s == null ? 0 : s.Balance
            };

        // เรียงตาม ProductCode เพื่อให้การแบ่งหน้าคงที่ทุกครั้งที่เรียก
        var items = await query
            .OrderBy(p => p.ProductCode)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return (items, totalCount);
    }
}
