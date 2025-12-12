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
    [HttpGet]
    public async Task<DefaultResponse<List<CategoryReadDTO>>> GetAll()
    {
        var categories = await categoryService.GetAll();
        return categories;
    }

   
    [HttpGet("{id:int}")]
    public async Task<DefaultResponse<List<CategoryReadDTO>>> GetById(int id)
    {
        var category = await categoryService.GetById(id);
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