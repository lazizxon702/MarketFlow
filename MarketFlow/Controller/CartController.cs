using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RootLibrary.DTO.CartDTO;
using RootLibrary.Enums;
using RootLibrary.Interface;
using RootLibrary.Models.Response;

namespace MarketFlow.Controller;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CartController(ICartService cartService) : ControllerBase
{
    private bool TryGetUserId(out int userId)
    {
        userId = 0;
        var claim = User.FindFirst("UserId"); 
        if (claim == null)
            return false;

        return int.TryParse(claim.Value, out userId);
    }

    [Authorize(Roles = "User,Admin")]
    [HttpGet("my-cart")]
    public async Task<DefaultResponse<List<CartReadDTO>>> GetUserCart()
    {
        if (!TryGetUserId(out int userId))
            return new DefaultResponse<List<CartReadDTO>>(new ErrorResponse { Message = "UserId claim not found" });

        return await cartService.GetUserCartAsync(userId);
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost("add")]
    public async Task<DefaultResponse<CartReadDTO>> AddToCart([FromBody] CartCreateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var error = new ErrorResponse("Validatsiya xatosi", (int)ResponseCode.ValidationError);
            return new DefaultResponse<CartReadDTO>(error);
        }

        if (!TryGetUserId(out int userId))
            return new DefaultResponse<CartReadDTO>(new ErrorResponse { Message = "UserId claim not found" });

        return await cartService.AddToCartAsync(userId, dto);
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPut("update")]
    public async Task<DefaultResponse<CartReadDTO>> UpdateCart([FromBody] CartUpdateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var error = new ErrorResponse("Validatsiya xatosi", (int)ResponseCode.ValidationError);
            return new DefaultResponse<CartReadDTO>(error);
        }

        if (!TryGetUserId(out int userId))
            return new DefaultResponse<CartReadDTO>(new ErrorResponse { Message = "UserId claim not found" });

        return await cartService.UpdateCartItemAsync(userId, dto);
    }

    [Authorize(Roles = "User,Admin")]
    [HttpDelete("remove/{cartId:int}")]
    public async Task<DefaultResponse<bool>> RemoveCartItem(int cartId)
    {
        if (!TryGetUserId(out int userId))
            return new DefaultResponse<bool>(new ErrorResponse { Message = "UserId claim not found" });

        return await cartService.RemoveCartItemAsync(userId, cartId);
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost("checkout")]
    public async Task<DefaultResponse<CartCheckoutDTO>> Checkout()
    {
        if (!TryGetUserId(out int userId))
            return new DefaultResponse<CartCheckoutDTO>(new ErrorResponse { Message = "UserId claim not found" });

        return await cartService.CheckoutAsync(userId);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all-orders")]
    public async Task<DefaultResponse<List<AdminOrderReadDTO>>> GetAllOrders()
    {
        return await cartService.GetAllOrdersAsync();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("cancel-order/{cartId:int}")]
    public async Task<DefaultResponse<bool>> CancelOrder(int cartId)
    {
        return await cartService.CancelOrderAsync(cartId);
    }
}
