using MarketFlow.DTO.CategoryDTO;
using MarketFlow.Models.Response;
using MarzketFlow.DTO.CategoryDTO;

namespace MarketFlow.Interface;

public interface ICategoryService
{
    Task<DefaultResponse<List<CategoryReadDTO>>>  GetAll();
    Task<DefaultResponse<List<CategoryReadDTO>>> GetById(int id);
    Task<DefaultResponse<string>> Create(CategoryCreateDTO dto);
    Task<DefaultResponse<bool>> Update(int id, CategoryUpdateDTO dto);
    Task<DefaultResponse<bool>> Delete(int id);
}