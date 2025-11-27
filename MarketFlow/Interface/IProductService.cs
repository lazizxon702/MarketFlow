using MarketFlow.DTO.ProductDTO;
using MarketFlow.Models.Response;

namespace MarketFlow.Interface;

public interface IProductService
{
    Task<DefaultResponse<List<ProductReadDTO>>> GetAll();
    Task<DefaultResponse<ProductReadDTO>> GetById(int id);
    Task<DefaultResponse<string>> Create(ProductCreateDTO dto);
    Task<DefaultResponse<bool>> Update(int id, ProductUpdateDTO dto);
    Task<DefaultResponse<bool>> Delete(int id);
}