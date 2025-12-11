using MarketFlow.DTO.OrderItemDTO;
using MarketFlow.Enums;
using MarketFlow.Interface;
using MarketFlow.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketFlow.Controller;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrderItemController(IOrderItemService orderItemService) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<DefaultResponse<List<OrderItemReadDTO>>> GetAll()
    {
        var orderItems = await orderItemService.GetAll();
        return orderItems;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}")]
    public async Task<DefaultResponse<OrderItemReadDTO>> GetById(int id)
    {
        var orderItem = await orderItemService.GetById(id);
        return orderItem;
    }

    
    [HttpPost]
    public async Task<DefaultResponse<string>> CreateOrderItem([FromBody] OrderItemCreateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var error = new ErrorResponse("Validatsiya xatosi", (int)ResponseCode.ValidationError);
            return new DefaultResponse<string>(error);
        }

        var result = await orderItemService.Create(dto);
        return result;
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<DefaultResponse<bool>> UpdateOrderItem(int id, [FromBody] OrderItemUpdateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var error = new ErrorResponse("Validatsiya xatosi", (int)ResponseCode.ValidationError);
            return new DefaultResponse<bool>(error);
        }

        var result = await orderItemService.Update(id, dto);
        return result;
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<DefaultResponse<bool>> Delete(int id)
    {
        var result = await orderItemService.Delete(id);
        return result;
    }
}