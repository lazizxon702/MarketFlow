using MarketPlace.DTO.CategoryDTO;
using MarketPlace.Models.Response;

namespace MarketPlace.Interface;

public interface ICategoryService
{
    Task<DefaultResponse<List<CategoryReadDTO>>>  GetAll();
    Task<DefaultResponse<List<CategoryReadDTO>>> GetById(int id);
    Task<DefaultResponse<string>> Create(CategoryCreateDTO dto);
    Task<DefaultResponse<bool>> Update(int id, CategoryUpdateDTO dto);
    Task<DefaultResponse<bool>> Delete(int id);
}