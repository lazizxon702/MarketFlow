using MarketPlace.DTO.OrderItemDTO;
using MarketPlace.Models.Response;

namespace MarketPlace.Interface;

public interface IOrderItemService
{
    Task<DefaultResponse<List<OrderItemReadDTO>>> GetAll();
    Task<DefaultResponse<OrderItemReadDTO>> GetById(int id);
    Task<DefaultResponse<string>> Create(OrderItemCreateDTO dto);
    Task<DefaultResponse<bool>> Update(int id, OrderItemUpdateDTO dto);
    Task<DefaultResponse<bool>> Delete(int id);
}