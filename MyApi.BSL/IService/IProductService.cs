using MyApi.DTO.Interfaces;
using MyApi.DTO.Models.ResponseModels;

namespace MyApi.BSL.IService;

public interface IProductService
{
    Task<PagedResult<ProductResponse>> GetProductsAsync(GetProductsRequest request);
}
