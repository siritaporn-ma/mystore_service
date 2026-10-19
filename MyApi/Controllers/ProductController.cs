using Microsoft.AspNetCore.Mvc;
using MyApi.BSL.IService;
using MyApi.DTO.Interfaces;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService productService, ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    // GET api/Product/GetProducts?page=1&pageSize=10
    [HttpGet]
    [Route("GetProducts")]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductsRequest request)
    {
        try
        {
            var result = await _productService.GetProductsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProducts failed (page={Page}, pageSize={PageSize}): {Message}", request.Page, request.PageSize, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, "เกิดข้อผิดพลาดภายในระบบ");
        }
    }
}
