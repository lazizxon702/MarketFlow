using Microsoft.EntityFrameworkCore;
using RootLibrary.Data;
using RootLibrary.DTO.CartDTO;
using RootLibrary.Interface;
using RootLibrary.Models.Inner;
using RootLibrary.Models.Response;

namespace RootLibrary.Services;

public class CartService(AppDbContext db) : ICartService
{
 

    public async Task<DefaultResponse<CartReadDTO>> AddToCartAsync(int userId, CartCreateDTO dto)
    {
        try
        {
            var existing = await db.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == dto.ProductId && !c.IsOrdered);

            if (existing != null)
            {
                existing.Quantity += dto.Quantity;
            }
            else
            {
                var cart = new Cart
                {
                    UserId = userId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    IsOrdered = false
                };
                await db.Carts.AddAsync(cart);
            }

            await db.SaveChangesAsync();

            var cartListResponse = await GetUserCartAsync(userId);
            if (!cartListResponse.Success || cartListResponse.Data == null)
                throw new Exception("Failed to retrieve cart");

            var addedItem = cartListResponse.Data.First(c => c.ProductId == dto.ProductId);

            return new DefaultResponse<CartReadDTO>(addedItem, "Item added to cart");
        }
        catch (Exception ex)
        {
            return new DefaultResponse<CartReadDTO>(new ErrorResponse { Message = ex.Message });
        }
    }

    public async Task<DefaultResponse<List<CartReadDTO>>> GetUserCartAsync(int userId)
    {
        try
        {
            var carts = await db.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId && !c.IsOrdered)
                .Select(c => new CartReadDTO
                {
                    Id = c.Id,
                    ProductId = c.ProductId,
                    ProductName = c.Product.NameS,
                    ProductDescription = c.Product.Description,
                    ProductPrice = c.Product.Price,
                    Quantity = c.Quantity,
                    IsOrdered = c.IsOrdered
                })
                .ToListAsync();

            return new DefaultResponse<List<CartReadDTO>>(carts, "User cart retrieved");
        }
        catch (Exception ex)
        {
            return new DefaultResponse<List<CartReadDTO>>(new ErrorResponse { Message = ex.Message });
        }
    }

    public async Task<DefaultResponse<CartReadDTO>> UpdateCartItemAsync(int userId, CartUpdateDTO dto)
    {
        try
        {
            var cart = await db.Carts
                .FirstOrDefaultAsync(c => c.Id == dto.Id && c.UserId == userId && !c.IsOrdered);

            if (cart == null)
                return new DefaultResponse<CartReadDTO>(new ErrorResponse { Message = "Item not found" });

            cart.Quantity = dto.Quantity;
            await db.SaveChangesAsync();

            var cartListResponse = await GetUserCartAsync(userId);
            var updatedItem = cartListResponse.Data.First(c => c.Id == dto.Id);

            return new DefaultResponse<CartReadDTO>(updatedItem, "Cart item updated");
        }
        catch (Exception ex)
        {
            return new DefaultResponse<CartReadDTO>(new ErrorResponse { Message = ex.Message });
        }
    }

    public async Task<DefaultResponse<bool>> RemoveCartItemAsync(int userId, int cartId)
    {
        try
        {
            var cart = await db.Carts
                .FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId && !c.IsOrdered);

            if (cart == null)
                return new DefaultResponse<bool>(new ErrorResponse { Message = "Item not found" });

            db.Carts.Remove(cart);
            await db.SaveChangesAsync();

            return new DefaultResponse<bool>(true, "Item removed from cart");
        }
        catch (Exception ex)
        {
            return new DefaultResponse<bool>(new ErrorResponse { Message = ex.Message });
        }
    }

    public async Task<DefaultResponse<CartCheckoutDTO>> CheckoutAsync(int userId)
    {
        try
        {
            var carts = await db.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId && !c.IsOrdered)
                .ToListAsync();

            if (!carts.Any())
                return new DefaultResponse<CartCheckoutDTO>(new ErrorResponse { Message = "Cart is empty" });

            foreach (var c in carts) c.IsOrdered = true;
            await db.SaveChangesAsync();

            var checkoutResult = new CartCheckoutDTO
            {
                UserId = userId,
                TotalItems = carts.Sum(c => c.Quantity),
                TotalPrice = carts.Sum(c => c.Quantity * c.Product.Price),
                CheckoutDate = DateTime.UtcNow
            };

            return new DefaultResponse<CartCheckoutDTO>(checkoutResult, "Checkout successful");
        }
        catch (Exception ex)
        {
            return new DefaultResponse<CartCheckoutDTO>(new ErrorResponse { Message = ex.Message });
        }
    }

    public async Task<DefaultResponse<List<AdminOrderReadDTO>>> GetAllOrdersAsync()
    {
        try
        {
            var orders = await db.Carts
                .Include(c => c.User)
                .Include(c => c.Product)
                .Where(c => c.IsOrdered)
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new AdminOrderReadDTO
                {
                    Id = c.Id,
                    UserName = c.User.Username,
                    UserPhone = c.User.PhoneNumber,
                })
                .ToListAsync();
            return new DefaultResponse<List<AdminOrderReadDTO>>(orders, "All orders retrieved");
        }
        catch (Exception ex)
        {
            return new DefaultResponse<List<AdminOrderReadDTO>>(new ErrorResponse { Message = ex.Message });
        }
    }

    public async Task<DefaultResponse<bool>> CancelOrderAsync(int cartId)
    {
        try
        {
            var cart = await db.Carts.FindAsync(cartId);
            if (cart == null || !cart.IsOrdered)
                return new DefaultResponse<bool>(new ErrorResponse { Message = "Order not found" });

            cart.IsOrdered = false;
            await db.SaveChangesAsync();

            return new DefaultResponse<bool>(true, "Order cancelled successfully");
        }
        catch (Exception ex)
        {
            return new DefaultResponse<bool>(new ErrorResponse { Message = ex.Message });
        }
    }    
}
