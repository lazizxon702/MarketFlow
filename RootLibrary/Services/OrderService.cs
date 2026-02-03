using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RootLibrary.Data;
using RootLibrary.DTO.OrderDTO;
using RootLibrary.Enums;
using RootLibrary.Interface;
using RootLibrary.Models.Inner;
using RootLibrary.Models.Response;

namespace RootLibrary.Services;

public class OrderService(AppDbContext db, IHttpContextAccessor httpContextAccessor) : IOrderService
{
    public async Task<List<DefaultResponse<OrderReadDTO>>> GetAllAsync()
    {
        try
        {
            var orders = await db.Orders
                .Include(o => o.User)
                .Where(o => !o.IsDeleted)
                .Select(o => new OrderReadDTO
                {
                    Id = o.Id,
                    UserFullName = o.User.Username,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    PaymentType = o.PaymentType.ToString(),
                    Address = o.Address
                })
                .ToListAsync();

            var responseList = orders
                .Select(o => new DefaultResponse<OrderReadDTO>(o, "Buyurtma topildi"))
                .ToList();

            return responseList;
        }
        catch
        {
            return new List<DefaultResponse<OrderReadDTO>> {
                new DefaultResponse<OrderReadDTO>(
                    new ErrorResponse("Buyurtmalarni olishda xatolik yuz berdi", (int)ResponseCode.ServerError)
                )
            };
        }
    }

    public async Task<DefaultResponse<OrderReadDTO>> GetByIdAsync(int id)
    {
        try
        {
            var order = await db.Orders
                .Include(o => o.User)
                .Where(o => o.Id == id && !o.IsDeleted)
                .Select(o => new OrderReadDTO
                {
                    Id = o.Id,
                    UserFullName = o.User.Username,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    PaymentType = o.PaymentType.ToString(),
                    Address = o.Address
                })
                .FirstOrDefaultAsync();

            if (order == null)
                return new DefaultResponse<OrderReadDTO>(
                    new ErrorResponse("Buyurtma topilmadi", (int)ResponseCode.NotFound)
                 );

            return new DefaultResponse<OrderReadDTO>(order, "Buyurtma topildi");
        }
        catch
        {
            return new DefaultResponse<OrderReadDTO>(
                new ErrorResponse("Buyurtmani olishda xatolik yuz berdi", (int)ResponseCode.ServerError)
            );
        }
    }
    public async Task<DefaultResponse<List<OrderReadDTO>>> GetUserOrder()
    {
        try
        {
            
            var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
        
            if ( !int.TryParse(userIdClaim, out int userId))
                return new DefaultResponse<List<OrderReadDTO>>(
                    new ErrorResponse("Foydalanuvchi autentifikatsiya qilinmagan", (int)ResponseCode.Unauthorized)
                );

            var orders = await db.Orders
                .Include(o => o.User)
                .Where(o => o.UserId == userId && !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderReadDTO
                {
                    Id = o.Id,
                    UserFullName = o.User.Username,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    PaymentType = o.PaymentType.ToString(),
                    Address = o.Address
                })
                .ToListAsync();

            if (orders == null || orders.Count == 0)
                return new DefaultResponse<List<OrderReadDTO>>(
                    new ErrorResponse("Sizda hali buyurtmalar yo'q", (int)ResponseCode.NotFound)
                );

            return new DefaultResponse<List<OrderReadDTO>>(orders, "Sizning buyurtmalaringiz");
        }
        catch
        {
            return new DefaultResponse<List<OrderReadDTO>>(
                new ErrorResponse("Buyurtmalarni olishda xatolik yuz berdi", (int)ResponseCode.ServerError)
            );
        }
    }

    public async Task<DefaultResponse<string>> CreateAsync(OrderCreateDTO dto)
    {
        try
        {
            var user = await db.Users.FindAsync(dto.UserId);
            if (user == null)
                return new DefaultResponse<string>(
                    new ErrorResponse("Foydalanuvchi topilmadi", (int)ResponseCode.NotFound)
                );

            var order = new Order
            {
                UserId = dto.UserId,
                TotalAmount = dto.TotalAmount,
                PaymentType = Enum.Parse<PaymentType>(dto.PaymentType, true),
                Status = OrderStatus.Pending,
                Address = dto.Address,
                OrderDate = DateTime.UtcNow,
                IsDeleted = false
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            return new DefaultResponse<string>("Buyurtma muvaffaqiyatli yaratildi");
        }
        catch
        {
            return new DefaultResponse<string>(
                new ErrorResponse("Buyurtma yaratishda xatolik yuz berdi", (int)ResponseCode.ServerError)
            );
        }
    }

    public async Task<DefaultResponse<bool>> UpdateAsync(int id, OrderUpdateDTO dto)
    {
        try
        {
            var order = await db.Orders.FindAsync(id);
            if (order == null || order.IsDeleted)
                return new DefaultResponse<bool>(
                    new ErrorResponse("Buyurtma topilmadi", (int)ResponseCode.NotFound)
                );

            order.TotalAmount = dto.TotalAmount;
            order.Status = Enum.Parse<OrderStatus>(dto.Status, true);
            order.PaymentType = Enum.Parse<PaymentType>(dto.PaymentType, true);
            order.Address = dto.Address;

            await db.SaveChangesAsync();

            return new DefaultResponse<bool>(true, "Buyurtma muvaffaqiyatli yangilandi");
        }
        catch
        {
            return new DefaultResponse<bool>(
                new ErrorResponse("Buyurtmani yangilashda xatolik yuz berdi", (int)ResponseCode.ServerError)
            );
        }
    }

    public async Task<DefaultResponse<bool>> DeleteAsync(int id)
    {
        try
        {
            var order = await db.Orders.FindAsync(id);
            if (order == null || order.IsDeleted)
                return new DefaultResponse<bool>(
                    new ErrorResponse("Buyurtma topilmadi", (int)ResponseCode.NotFound)
                );

            order.IsDeleted = true;
            await db.SaveChangesAsync();

            return new DefaultResponse<bool>(true, "Buyurtma muvaffaqiyatli o'chirildi");
        }
        catch
        {
            return new DefaultResponse<bool>(
                new ErrorResponse("Buyurtmani o'chirishda xatolik yuz berdi", (int)ResponseCode.ServerError)
            );
        }
    }
    public async Task<DefaultResponse<List<OrderReadDTO>>> GetUserOrder(int userId)
    {
        try
        {
            var orders = await db.Orders
                .Include(o => o.User)
                .Where(o => o.UserId == userId && !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderReadDTO
                {
                    Id = o.Id,
                    UserFullName = o.User.Username,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    PaymentType = o.PaymentType.ToString(),
                    Address = o.Address
                })
                .ToListAsync();

            if (orders == null || orders.Count == 0)
                return new DefaultResponse<List<OrderReadDTO>>(
                    new ErrorResponse("Sizda hali buyurtmalar yo'q", (int)ResponseCode.NotFound)
                );

            return new DefaultResponse<List<OrderReadDTO>>(orders, "Sizning buyurtmalaringiz");
        }
        catch
        {
            return new DefaultResponse<List<OrderReadDTO>>(
                new ErrorResponse("Buyurtmalarni olishda xatolik yuz berdi", (int)ResponseCode.ServerError)
            );
        }
    }

    public async Task<DefaultResponse<string>> CreateOrderFromCartAsync(int userId, PaymentType paymentType, string address)
    {
        try
        {
            var cartItems = await db.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId && !c.IsOrdered)
                .ToListAsync();

            if (!cartItems.Any())
                return new DefaultResponse<string>(new ErrorResponse("Savat bo‘sh", (int)ResponseCode.NotFound));

            decimal totalAmount = cartItems.Sum(c => c.Quantity * c.Product.Price);

            var order = new Order
            {
                UserId = userId,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                PaymentType = paymentType,
                Address = address,
                OrderDate = DateTime.Now,
                IsDeleted = false
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            // Add OrderItems
            foreach (var cart in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cart.ProductId,
                    Quantity = cart.Quantity,
                    ItemPrice = cart.Product.Price,
                    TotalPrice = cart.Quantity * cart.Product.Price
                };
                db.OrderItems.Add(orderItem);
                
                // Mark cart as ordered
                cart.IsOrdered = true;
            }

            await db.SaveChangesAsync();

            return new DefaultResponse<string>("Buyurtma muvaffaqiyatli qabul qilindi!");
        }
        catch (Exception ex)
        {
             return new DefaultResponse<string>(
                new ErrorResponse($"Xatolik: {ex.Message}", (int)ResponseCode.ServerError)
            );
        }
    }
}