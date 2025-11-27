using MarketFlow.DTO.OrderDTO;
using MarketFlow.Models.Response;
using MarketPlace.DTO;

namespace MarketFlow.Interface;

public interface IOrderService
{
    Task<List<DefaultResponse<OrderReadDTO>>> GetAllAsync();
    
    Task<DefaultResponse<OrderReadDTO>> GetByIdAsync(int id);
    
    Task<DefaultResponse<List<OrderReadDTO>>> GetUserOrder();
    
    Task<DefaultResponse<string>> CreateAsync(OrderCreateDTO dto);
    
    Task<DefaultResponse<bool>> UpdateAsync(int id, OrderUpdateDTO dto);
    
    Task<DefaultResponse<bool>> DeleteAsync(int id);
}