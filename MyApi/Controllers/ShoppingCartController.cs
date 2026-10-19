using Microsoft.AspNetCore.Mvc;
using MyApi.BSL.Exceptions;
using MyApi.BSL.IService;
using MyApi.DTO.Interfaces;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShoppingCartController : ControllerBase
{
    private readonly IShoppingCartService _shoppingCartService;
    private readonly ILogger<ShoppingCartController> _logger;

    public ShoppingCartController(IShoppingCartService shoppingCartService, ILogger<ShoppingCartController> logger)
    {
        _shoppingCartService = shoppingCartService;
        _logger = logger;
    }

    // POST api/ShoppingCart/CreateCart  (body = รายการสินค้า) สร้างตะกร้าและตัดสต็อกตาม Qty ในคราวเดียว
    // 400 = ข้อมูล/สินค้า/สต็อกไม่ถูกต้อง, 409 = (OrderNo, ProductCode) ซ้ำกับที่มีอยู่, 500 = ข้อผิดพลาดภายใน
    [HttpPost]
    [Route("CreateCart")]
    public async Task<IActionResult> CreateCart([FromBody] List<CreateCartItemRequest> request)
    {
        try
        {
            var result = await _shoppingCartService.CreateCartAsync(request);
            return Ok(result);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ConflictException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateCart failed (items={Count}): {Message}", request?.Count ?? 0, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, "เกิดข้อผิดพลาดภายในระบบ");
        }
    }

    // GET api/ShoppingCart/GetListOrder?page=1&pageSize=10  (ไม่ส่ง = page 1, pageSize 10)
    [HttpGet]
    [Route("GetListOrder")]
    public async Task<IActionResult> GetListOrder([FromQuery] GetListOrderRequest request)
    {
        try
        {
            var result = await _shoppingCartService.GetListOrderAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetListOrder failed (page={Page}, pageSize={PageSize}): {Message}", request.Page, request.PageSize, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, "เกิดข้อผิดพลาดภายในระบบ");
        }
    }
}
