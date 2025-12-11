using MarketFlow.DTO.UserDTO;
using MarketFlow.Enums;
using MarketFlow.Interface;
using MarketFlow.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketFlow.Controller;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<DefaultResponse<List<UserReadDTO>>> GetAll()
    {
        var users = await userService.GetAll();
        return users;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}")]
    public async Task<DefaultResponse<UserReadDTO>> GetById(int id)
    {
        var user = await userService.GetById(id);
        return user;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<DefaultResponse<string>> CreateUser([FromBody] UserCreateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var error = new ErrorResponse("Validatsiya xatosi", (int)ResponseCode.ValidationError);
            return new DefaultResponse<string>(error);
        }

        var result = await userService.CreateUser(dto);
        return result;
    }
    
    [HttpPut("{id:int}")]
    public async Task<DefaultResponse<bool>> UpdateUser(int id, UserUpdateDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var error = new ErrorResponse("Validatsiya xatosi", (int)ResponseCode.ValidationError);
            return new DefaultResponse<bool>(error);
        }

        var result = await userService.UpdateUser(id, dto);
        return result;
    }
    
    [HttpDelete("{id:int}")]
    public async Task<DefaultResponse<bool>> Delete(int id)
    {
        var result = await userService.Delete(id);
        return result;
    }
}