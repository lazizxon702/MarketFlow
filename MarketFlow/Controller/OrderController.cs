using MarketFlow.DTO.OrderDTO;
using MarketFlow.Enums;
using MarketFlow.Interface;
using MarketFlow.Models.Response;
using MarketPlace.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Controller;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrderController(IOrderService orderService) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<List<DefaultResponse<OrderReadDTO>>> GetAll()
    {
        var orders = await orderService.GetAllAsync();
        return orders;
    }

   
    [HttpGet("{id:int}")]
    public async Task<DefaultResponse<OrderReadDTO>> GetById(int id)
    {
        var order = await orderService.GetByIdAsync(id);
        return order;
    }

    [HttpGet("my-orders")]
    public async Task<DefaultResponse<List<OrderReadDTO>>> GetUserOrder()
    {
        var orders = await orderService.GetUserOrder();
        return orders;
    }

    
    [HttpPost]
    public async Task<DefaultResponse<string>> CreateOrder([FromBody] OrderCreateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var error = new ErrorResponse("Validatsiya xatosi", (int)ResponseCode.ValidationError);
            return new DefaultResponse<string>(error);
        }

        var result = await orderService.CreateAsync(dto);
        return result;
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<DefaultResponse<bool>> UpdateOrder(int id, [FromBody] OrderUpdateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var error = new ErrorResponse("Validatsiya xatosi", (int)ResponseCode.ValidationError);
            return new DefaultResponse<bool>(error);
        }

        var result = await orderService.UpdateAsync(id, dto);
        return result;
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<DefaultResponse<bool>> Delete(int id)
    {
        var result = await orderService.DeleteAsync(id);
        return result;
    }
}