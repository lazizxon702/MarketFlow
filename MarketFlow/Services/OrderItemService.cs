using MarketFlow;
using MarketPlace.DTO.OrderItemDTO;
using MarketPlace.Interface;
using MarketPlace.Models.Response;
using MarketPlace.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Services;

public class OrderItemService(AppDbContext db) : IOrderItemService
{
    public async Task<DefaultResponse<List<OrderItemReadDTO>>> GetAll()
    {
        try
        {
            var items = await db.OrderItems
                .Include(oi => oi.Product)
                .Select(i => new OrderItemReadDTO
                {
                    Id = i.Id,
                    ProductName = i.Product.NameS,
                    Quantity = i.Quantity,
                    Price = i.ItemPrice
                })
                .ToListAsync();

            return new DefaultResponse<List<OrderItemReadDTO>>(items, "Buyurtma itemlari topildi");
        }
        catch
        {
            return new DefaultResponse<List<OrderItemReadDTO>>(
                new ErrorResponse("Buyurtma itemlarini olishda xatolik yuz berdi", (int)ResponseCode.ServerError)
            );
        }
    }

    public async Task<DefaultResponse<OrderItemReadDTO>> GetById(int id)
    {
        try
        {
            var item = await db.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.Id == id)
                .Select(i => new OrderItemReadDTO
                {
                    Id = i.Id,
                    ProductName = i.Product.NameS,
                    Quantity = i.Quantity,
                    Price = i.ItemPrice
                })
                .FirstOrDefaultAsync();

            if (item == null)
                return new DefaultResponse<OrderItemReadDTO>(
                    new ErrorResponse("Buyurtma itemi topilmadi", (int)ResponseCode.NotFound)
                );

            return new DefaultResponse<OrderItemReadDTO>(item, "Buyurtma itemi topildi");
        }
        catch
        {
            return new DefaultResponse<OrderItemReadDTO>(
                new ErrorResponse("Buyurtma itemini olishda xatolik yuz berdi", (int)ResponseCode.ServerError)
            );
        }
    }

    public async Task<DefaultResponse<string>> Create(OrderItemCreateDTO dto)
    {
        try
        {
            var order = await db.Orders.FindAsync(dto.OrderId);
            if (order == null)
                return new DefaultResponse<string>(
                    new ErrorResponse("Buyurtma topilmadi", (int)ResponseCode.NotFound)
                );

            var product = await db.Products.FindAsync(dto.ProductId);
            if (product == null)
                return new DefaultResponse<string>(
                    new ErrorResponse("Mahsulot topilmadi", (int)ResponseCode.NotFound)
                );

            var orderItem = new OrderItem
            {
                OrderId = dto.OrderId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                ItemPrice = product.Price * dto.Quantity
            };

            db.OrderItems.Add(orderItem);
            order.TotalAmount += orderItem.ItemPrice;
            await db.SaveChangesAsync();

            return new DefaultResponse<string>("Buyurtma itemi muvaffaqiyatli qo'shildi");
        }
        catch
        {
            return new DefaultResponse<string>(
                new ErrorResponse("Buyurtma itemini yaratishda xatolik yuz berdi", (int)ResponseCode.ServerError)
            );
        }
    }

    public async Task<DefaultResponse<bool>> Update(int id, OrderItemUpdateDTO dto)
    {
        try
        {
            var item = await db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .FirstOrDefaultAsync(oi => oi.Id == id);

            if (item == null)
                return new DefaultResponse<bool>(
                    new ErrorResponse("Buyurtma itemi topilmadi", (int)ResponseCode.NotFound)
                );

            item.Order.TotalAmount -= item.ItemPrice;
            item.Quantity = dto.Quantity;
            item.ItemPrice = item.Product.Price * dto.Quantity;
            item.Order.TotalAmount += item.ItemPrice;

            await db.SaveChangesAsync();

            return new DefaultResponse<bool>(true, "Buyurtma itemi muvaffaqiyatli yangilandi");
        }
        catch
        {
            return new DefaultResponse<bool>(
                new ErrorResponse("Buyurtma itemini yangilashda xatolik yuz berdi", (int)ResponseCode.ServerError)
            );
        }
    }

    public async Task<DefaultResponse<bool>> Delete(int id)
    {
        try
        {
            var item = await db.OrderItems
                .Include(oi => oi.Order)
                .FirstOrDefaultAsync(oi => oi.Id == id);

            if (item == null)
                return new DefaultResponse<bool>(
                    new ErrorResponse("Buyurtma itemi topilmadi", (int)ResponseCode.NotFound)
                );

            item.Order.TotalAmount -= item.ItemPrice;
            db.OrderItems.Remove(item);
            await db.SaveChangesAsync();

            return new DefaultResponse<bool>(true, "Buyurtma itemi muvaffaqiyatli o'chirildi");
        }
        catch
        {
            return new DefaultResponse<bool>(
                new ErrorResponse("Buyurtma itemini o'chirishda xatolik yuz berdi", (int)ResponseCode.ServerError)
            );
        }
    }
}