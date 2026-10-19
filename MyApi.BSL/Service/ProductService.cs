using MyApi.BSL.IService;
using MyApi.DAL.IData;
using MyApi.DTO.Interfaces;
using MyApi.DTO.Models.ResponseModels;

namespace MyApi.BSL.Service;

public class ProductService : IProductService
{
    private const int MaxPageSize = 100;

    private readonly IProductData _productData;

    public ProductService(IProductData productData)
    {
        _productData = productData;
    }

    public async Task<PagedResult<ProductResponse>> GetProductsAsync(GetProductsRequest request)
    {
        // ปรับค่า: page < 1 -> 1, pageSize อยู่ระหว่าง 1..MaxPageSize
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);

        // ใช้ long กัน overflow เมื่อ page มีค่าสูงมาก
        var skipLong = (long)(page - 1) * pageSize;
        var skip = skipLong > int.MaxValue ? int.MaxValue : (int)skipLong;

        var (products, totalCount) = await _productData.GetPagedAsync(skip, pageSize);

        return new PagedResult<ProductResponse>
        {
            Items = products,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}
