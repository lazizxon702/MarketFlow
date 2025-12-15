using MarketFlow.DTO.CategoryDTO;
using MarketFlow.Enums;
using MarketFlow.Interface;
using MarketFlow.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketFlow.Controller;

[Route("api/[controller]")]
[ApiController]
[Authorize] 
public class CategoryController(ICategoryService categoryService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet( "GetMainCategories")]
    public async Task<DefaultResponse<List<CategoryReadDTO>>> GetMainCategories()
    {
        var categories = await categoryService.GetMainCategories();
        return categories;
    }

    [AllowAnonymous]
    [HttpGet("{id:int}", Name = "GetChildCategories")]
    public async Task<DefaultResponse<CategoryReadDTO>> GetChildCategories(int id)
    {
        var category = await categoryService.GetChildCategories(id);
        return category;
    }


    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<DefaultResponse<string>> CreateCategory([FromBody] CategoryCreateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var error = new ErrorResponse("Validatsiya xatosi", (int)ResponseCode.ValidationError);
            return new DefaultResponse<string>(error);
        }

        var result = await categoryService.Create(dto);
        return result;
    }

   
    [Authorize(Roles = "Admin")] 
    [HttpPut("{id:int}")]
    public async Task<DefaultResponse<bool>> UpdateCategory(int id, [FromBody] CategoryUpdateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var error = new ErrorResponse("Validatsiya xatosi", (int)ResponseCode.ValidationError);
            return new DefaultResponse<bool>(error);
        }

        var updated = await categoryService.Update(id, dto);
        return updated;
    }

    
    [Authorize(Roles = "Admin")] 
    [HttpDelete("{id:int}")]
    public async Task<DefaultResponse<bool>> DeleteCategory(int id)
    {
        var deleted = await categoryService.Delete(id);
        return deleted;
    }
}