
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RootLibrary.DTO.ProductDTO;
using RootLibrary.Enums;
using RootLibrary.Interface;
using RootLibrary.Models.Response;

namespace MarketFlow.Controller;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ProductController(IProductService productService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<DefaultResponse<List<ProductReadDTO>>> GetAll()
    {
        var products = await productService.GetAll();
        return products;
    }

    [HttpGet("{id:int}")]
    public async Task<DefaultResponse<ProductReadDTO>> GetById(int id)
    {
        var product = await productService.GetById(id);
        return product;
    }
    
    [HttpPost]
    public async Task<DefaultResponse<string>> CreateProduct([FromBody] ProductCreateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var error = new ErrorResponse("Validatsiya xatosi", (int)ResponseCode.BadRequest);
            return new DefaultResponse<string>(error);
        }

        var result = await productService.Create(dto);
        return result;
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<DefaultResponse<bool>> UpdateProduct(int id, ProductUpdateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var error = new ErrorResponse("Validatsiya xatosi", (int)ResponseCode.BadRequest);
            return new DefaultResponse<bool>(error);
        }

        var result = await productService.Update(id, dto);
        return result;
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<DefaultResponse<bool>> Delete(int id)
    {
        var result = await productService.Delete(id);
        return result;
    }
}