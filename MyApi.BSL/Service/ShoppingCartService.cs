using MyApi.BSL.Exceptions;
using MyApi.BSL.IService;
using MyApi.DAL.IData;
using MyApi.DTO.Interfaces;
using MyApi.DTO.Models.MainModels;
using MyApi.DTO.Models.ResponseModels;

namespace MyApi.BSL.Service;

public class ShoppingCartService : IShoppingCartService
{
    private const int MaxLength = 50;
    private const int MaxPageSize = 100;
    private const decimal MaxPrice = 9999999999999999.99m; // decimal(18,2)

    private readonly IShoppingCartData _shoppingCartData;

    public ShoppingCartService(IShoppingCartData shoppingCartData)
    {
        _shoppingCartData = shoppingCartData;
    }

    public async Task<IReadOnlyList<ShoppingCartResponse>> CreateCartAsync(List<CreateCartItemRequest>? request)
    {
        if (request == null || request.Count == 0)
        {
            throw new BadRequestException("ต้องส่งรายการสินค้าอย่างน้อย 1 รายการ");
        }

        // ตรวจและ Trim ทุกรายการก่อน; entity สร้างใหม่เองโดยไม่คัดลอก audit fields จาก client
        var items = new List<ShoppingCart>(request.Count);
        for (var i = 0; i < request.Count; i++)
        {
            items.Add(ToEntity(request[i], i + 1));
        }

        // (OrderNo, ProductCode) ห้ามซ้ำกันในคำขอ (เทียบไม่สนตัวพิมพ์เล็กใหญ่ให้ตรงกับ DB)
        var duplicates = items
            .GroupBy(x => (x.OrderNo.ToUpperInvariant(), x.ProductCode.ToUpperInvariant()))
            .Where(g => g.Count() > 1)
            .Select(g => $"({g.First().OrderNo}, {g.First().ProductCode})")
            .ToList();
        if (duplicates.Count > 0)
        {
            throw new BadRequestException($"พบรายการซ้ำในคำขอ (OrderNo, ProductCode): {string.Join(", ", duplicates)}");
        }

        // ตรวจว่า ProductCode ทุกตัวมีอยู่จริง (ไม่มี FK จึงตรวจเอง; query เดียว)
        var productCodes = items
            .Select(x => x.ProductCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var existingCodes = await _shoppingCartData.GetExistingProductCodesAsync(productCodes);
        var existingCodeSet = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);
        var missingCodes = productCodes.Where(c => !existingCodeSet.Contains(c)).ToList();
        if (missingCodes.Count > 0)
        {
            throw new BadRequestException($"ไม่พบรหัสสินค้า: {string.Join(", ", missingCodes)}");
        }

        // ตรวจว่ายังไม่มี (OrderNo, ProductCode) ในตะกร้า (query เดียว)
        var keys = items.Select(x => (x.OrderNo, x.ProductCode)).ToList();
        var existingKeys = await _shoppingCartData.GetExistingKeysAsync(keys);
        if (existingKeys.Count > 0)
        {
            var existing = existingKeys.Select(k => $"({k.OrderNo}, {k.ProductCode})");
            throw new ConflictException($"มีรายการในตะกร้าอยู่แล้ว (OrderNo, ProductCode): {string.Join(", ", existing)}");
        }

        // ยอดที่ต้องใช้ต่อสินค้า (สินค้าเดียวกันอาจอยู่หลาย order ในคำขอเดียว จึงรวม Qty ก่อนตรวจ)
        var qtyByCode = items
            .GroupBy(x => x.ProductCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Sum(x => (long)x.Qty), StringComparer.OrdinalIgnoreCase);

        // ตรวจล่วงหน้าเพื่อให้ได้ข้อความ error ชัดเจน (ตัวกัน oversell จริงคือ update แบบมีเงื่อนไขใน DAL)
        var balances = await _shoppingCartData.GetStockBalancesAsync(productCodes);
        var balanceByCode = balances.ToDictionary(b => b.ProductCode, b => b.Balance, StringComparer.OrdinalIgnoreCase);

        var noStock = productCodes.Where(c => !balanceByCode.ContainsKey(c)).ToList();
        if (noStock.Count > 0)
        {
            throw new BadRequestException($"ไม่พบสต็อกของสินค้า: {string.Join(", ", noStock)}");
        }

        var insufficient = productCodes
            .Where(c => balanceByCode[c] < qtyByCode[c])
            .Select(c => $"{c} (คงเหลือ {balanceByCode[c]}, ต้องการ {qtyByCode[c]})")
            .ToList();
        if (insufficient.Count > 0)
        {
            throw new BadRequestException($"สต็อกไม่เพียงพอ: {string.Join(", ", insufficient)}");
        }

        // บันทึกตะกร้า + ตัดสต็อกใน transaction เดียว; false = มีคำขออื่นตัดสต็อกไปก่อนจนไม่พอ (rollback แล้ว ตะกร้าไม่ถูกบันทึก)
        var saved = await _shoppingCartData.AddRangeAndDeductStockAsync(items);
        if (!saved)
        {
            throw new BadRequestException("สต็อกไม่เพียงพอ (สต็อกเปลี่ยนแปลงระหว่างดำเนินการ) กรุณาลองใหม่อีกครั้ง");
        }

        var orderNos = items
            .Select(x => x.OrderNo)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        return await _shoppingCartData.GetByOrderNosAsync(orderNos);
    }

    public async Task<PagedResult<ShoppingCartResponse>> GetListOrderAsync(GetListOrderRequest request)
    {
        // ปรับค่า: page < 1 -> 1, pageSize อยู่ระหว่าง 1..MaxPageSize
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);

        // ใช้ long กัน overflow เมื่อ page มีค่าสูงมาก
        var skipLong = (long)(page - 1) * pageSize;
        var skip = skipLong > int.MaxValue ? int.MaxValue : (int)skipLong;

        var (items, totalCount) = await _shoppingCartData.GetListPagedAsync(skip, pageSize);

        return new PagedResult<ShoppingCartResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    // ตรวจ 1 รายการ + Trim แล้วสร้าง ShoppingCart ใหม่; position ใช้บอกลำดับ (เริ่มที่ 1) ในข้อความ error
    private static ShoppingCart ToEntity(CreateCartItemRequest? item, int position)
    {
        if (item == null)
        {
            throw new BadRequestException($"รายการที่ {position} ต้องไม่เป็นค่าว่าง");
        }

        var orderNo = item.OrderNo?.Trim() ?? string.Empty;
        var productCode = item.ProductCode?.Trim() ?? string.Empty;

        if (orderNo.Length == 0)
        {
            throw new BadRequestException($"รายการที่ {position}: OrderNo ต้องไม่ว่าง");
        }
        if (orderNo.Length > MaxLength)
        {
            throw new BadRequestException($"รายการที่ {position}: OrderNo ต้องยาวไม่เกิน {MaxLength} ตัวอักษร");
        }
        if (productCode.Length == 0)
        {
            throw new BadRequestException($"รายการที่ {position}: ProductCode ต้องไม่ว่าง");
        }
        if (productCode.Length > MaxLength)
        {
            throw new BadRequestException($"รายการที่ {position}: ProductCode ต้องยาวไม่เกิน {MaxLength} ตัวอักษร");
        }
        if (item.Qty <= 0)
        {
            throw new BadRequestException($"รายการที่ {position}: Qty ต้องมากกว่า 0");
        }
        if (item.Price < 0 || item.Price > MaxPrice)
        {
            throw new BadRequestException($"รายการที่ {position}: Price ต้องอยู่ระหว่าง 0 ถึง {MaxPrice}");
        }

        return new ShoppingCart
        {
            OrderNo = orderNo,
            ProductCode = productCode,
            Qty = item.Qty,
            Price = item.Price
        };
    }
}
