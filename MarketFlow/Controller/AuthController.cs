using MarketFlow.DTO.AuthDTO;
using MarketFlow.Enums;
using MarketFlow.Interface;
using MarketFlow.Models.Response;
using MarketPlace.DTO.Auth;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{

    [HttpPost("signup")]
    public async Task<DefaultResponse<string>> SignUp([FromBody] AuthRegisterDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var error = new ErrorResponse("Bunday foydalanuvchi mavjud ", (int)ResponseCode.BadRequest);
            return new DefaultResponse<string>(error);
        }

        var result = await authService.SignUp(dto);
        return result;
    }

    [HttpPost("login")]
    public async Task<DefaultResponse<string>> Login([FromBody] AuthLoginDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var error = new ErrorResponse("Kechirasiz bunday Login topilmadi!!!", (int)ResponseCode.BadRequest);
            return new DefaultResponse<string>(error);
        }

        var result = await authService.Login(dto);
        return result;
    }
}