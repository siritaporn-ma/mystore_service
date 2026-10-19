using Microsoft.EntityFrameworkCore;
using MyApi.DAL.Connections;
using MyApi.DAL.IData;
using MyApi.DTO.Models.BaseModels;
using MyApi.DTO.Models.MainModels;
using MyApi.DTO.Models.ResponseModels;

namespace MyApi.DAL.Data;

public class ShoppingCartData : IShoppingCartData
{
    private readonly ApplicationDBContext _context;

    public ShoppingCartData(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<string>> GetExistingProductCodesAsync(IReadOnlyCollection<string> productCodes)
    {
        var codes = productCodes.ToList();

        return await _context.Products
            .AsNoTracking()
            .Where(p => codes.Contains(p.ProductCode))
            .Select(p => p.ProductCode)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<(string OrderNo, string ProductCode)>> GetExistingKeysAsync(IReadOnlyCollection<(string OrderNo, string ProductCode)> keys)
    {
        var orderNos = keys.Select(k => k.OrderNo).Distinct().ToList();
        var productCodes = keys.Select(k => k.ProductCode).Distinct().ToList();

        // query เดียวดึงชุดที่ครอบคลุม (OrderNo x ProductCode) แล้วกรองเฉพาะคู่ที่ตรงจริงในหน่วยความจำ
        var candidates = await _context.ShoppingCarts
            .AsNoTracking()
            .Where(c => orderNos.Contains(c.OrderNo) && productCodes.Contains(c.ProductCode))
            .Select(c => new { c.OrderNo, c.ProductCode })
            .ToListAsync();

        // SQL Server เทียบตัวพิมพ์ไม่สนตัวพิมพ์เล็กใหญ่ (collation ปกติ) จึงเทียบแบบ OrdinalIgnoreCase ให้ตรงกัน
        var requested = new HashSet<(string, string)>(keys, KeyComparer.Instance);

        return candidates
            .Where(c => requested.Contains((c.OrderNo, c.ProductCode)))
            .Select(c => (c.OrderNo, c.ProductCode))
            .ToList();
    }

    public async Task<bool> AddRangeAndDeductStockAsync(IReadOnlyCollection<ShoppingCart> items)
    {
        var now = DateTime.UtcNow;

        // ไม่ใช้ retry-on-failure ในโปรเจกต์ จึงใช้ transaction ธรรมดา; ออกจาก scope โดยไม่ commit = rollback อัตโนมัติ
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.ShoppingCarts.AddRange(items);
            await _context.SaveChangesAsync();

            // เรียงตาม ProductCode เสมอ เพื่อให้ลำดับการล็อกแถวคงที่ ลดโอกาส deadlock เมื่อมีหลายคำขอพร้อมกัน
            foreach (var item in items.OrderBy(i => i.ProductCode, StringComparer.OrdinalIgnoreCase))
            {
                var productCode = item.ProductCode;
                var qty = item.Qty;

                // เงื่อนไข Balance >= qty ใน SQL เป็นตัวกัน oversell จริง (atomic ต่อแถว)
                var affected = await _context.Stocks
                    .Where(s => s.ProductCode == productCode && s.Balance >= qty)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.Balance, s => s.Balance - qty)
                        .SetProperty(s => s.UpdatedBy, BaseModel.DefaultCreatedBy)
                        .SetProperty(s => s.UpdatedDate, (DateTime?)now));

                if (affected == 0)
                {
                    await transaction.RollbackAsync();
                    DetachItems(items);
                    return false;
                }
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            // transaction จะ rollback เองเมื่อ dispose; เคลียร์ entity ที่ค้างใน change tracker ไม่ให้ scoped DbContext สกปรก
            DetachItems(items);
            throw;
        }
    }

    // ถอด entity ที่เพิ่งเพิ่มออกจาก change tracker (ใช้หลัง rollback)
    private void DetachItems(IReadOnlyCollection<ShoppingCart> items)
    {
        foreach (var item in items)
        {
            _context.Entry(item).State = EntityState.Detached;
        }
    }

    public async Task<IReadOnlyList<ShoppingCartResponse>> GetByOrderNosAsync(IReadOnlyCollection<string> orderNos)
    {
        var orders = orderNos.ToList();

        // ไม่มี FK จึง join ด้วย ProductCode; แถวที่ไม่มีสินค้าจะถูกตัดออกโดย inner join
        var query =
            from c in _context.ShoppingCarts.AsNoTracking()
            join p in _context.Products.AsNoTracking() on c.ProductCode equals p.ProductCode
            where orders.Contains(c.OrderNo)
            select new ShoppingCartResponse
            {
                OrderNo = c.OrderNo,
                ProductCode = c.ProductCode,
                ProductName = p.ProductName,
                Qty = c.Qty,
                Price = c.Price
            };

        return await query
            .OrderBy(r => r.OrderNo)
            .ThenBy(r => r.ProductCode)
            .ToListAsync();
    }

    public async Task<(IReadOnlyList<ShoppingCartResponse> Items, int TotalCount)> GetListPagedAsync(int skip, int take)
    {
        // ไม่มี FK จึง join ด้วย ProductCode; แถวที่ไม่มีสินค้าจะถูกตัดออกโดย inner join
        var query =
            from c in _context.ShoppingCarts.AsNoTracking()
            join p in _context.Products.AsNoTracking() on c.ProductCode equals p.ProductCode
            select new ShoppingCartResponse
            {
                OrderNo = c.OrderNo,
                ProductCode = c.ProductCode,
                ProductName = p.ProductName,
                Qty = c.Qty,
                Price = c.Price
            };

        var totalCount = await query.CountAsync();

        // เรียงตาม OrderNo แล้ว ProductCode (คีย์หลัก) เพื่อให้การแบ่งหน้าคงที่ทุกครั้งที่เรียก
        var items = await query
            .OrderBy(r => r.OrderNo)
            .ThenBy(r => r.ProductCode)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<(string ProductCode, int Balance)>> GetStockBalancesAsync(IReadOnlyCollection<string> productCodes)
    {
        var codes = productCodes.ToList();

        var rows = await _context.Stocks
            .AsNoTracking()
            .Where(s => codes.Contains(s.ProductCode))
            .Select(s => new { s.ProductCode, s.Balance })
            .ToListAsync();

        return rows.Select(r => (r.ProductCode, r.Balance)).ToList();
    }

    // เปรียบเทียบคู่ (OrderNo, ProductCode) แบบไม่สนตัวพิมพ์เล็กใหญ่
    private sealed class KeyComparer : IEqualityComparer<(string, string)>
    {
        public static readonly KeyComparer Instance = new();

        public bool Equals((string, string) x, (string, string) y) =>
            StringComparer.OrdinalIgnoreCase.Equals(x.Item1, y.Item1) &&
            StringComparer.OrdinalIgnoreCase.Equals(x.Item2, y.Item2);

        public int GetHashCode((string, string) obj) =>
            HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item1),
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item2));
    }
}
