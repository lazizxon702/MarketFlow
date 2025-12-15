using MarketFlow.DTO.CategoryDTO;
using MarketFlow.Models.Response;

namespace MarketFlow.Interface;

public interface ICategoryService
{
    Task<DefaultResponse<List<CategoryReadDTO>>>  GetMainCategories();
    Task<DefaultResponse<CategoryReadDTO>> GetChildCategories(int mainId);
    Task<DefaultResponse<string>> Create(CategoryCreateDTO dto);
    Task<DefaultResponse<bool>> Update(int id, CategoryUpdateDTO dto);
    Task<DefaultResponse<bool>> Delete(int id);
}