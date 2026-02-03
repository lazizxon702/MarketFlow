using RootLibrary.DTO.OrderDTO;
using RootLibrary.Enums;
using RootLibrary.Models.Response;

namespace RootLibrary.Interface;

public interface IOrderService
{
    Task<List<DefaultResponse<OrderReadDTO>>> GetAllAsync();
    
    Task<DefaultResponse<OrderReadDTO>> GetByIdAsync(int id);
    
    Task<DefaultResponse<List<OrderReadDTO>>> GetUserOrder();
    
    Task<DefaultResponse<string>> CreateAsync(OrderCreateDTO dto);
    
    Task<DefaultResponse<bool>> UpdateAsync(int id, OrderUpdateDTO dto);
    
    Task<DefaultResponse<bool>> DeleteAsync(int id);

    // Bot specific methods
    Task<DefaultResponse<List<OrderReadDTO>>> GetUserOrder(int userId);
    Task<DefaultResponse<string>> CreateOrderFromCartAsync(int userId, PaymentType paymentType, string address);
}