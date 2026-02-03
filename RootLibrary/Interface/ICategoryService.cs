using RootLibrary.DTO.CategoryDTO;
using RootLibrary.Models.Response;

namespace RootLibrary.Interface;

public interface ICategoryService
{
    Task<DefaultResponse<List<CategoryReadDTO>>>  GetMainCategories();
    Task<DefaultResponse<CategoryChildDTO>> GetChildCategories(int mainId);
    Task<DefaultResponse<string>> Create(CategoryCreateDTO dto);
    Task<DefaultResponse<bool>> Update(int id, CategoryUpdateDTO dto);
    Task<DefaultResponse<bool>> Delete(int id);
}