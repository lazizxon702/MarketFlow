using MarketFlow.DTO.AuthDTO;
using MarketFlow.Models.Response;
using MarketPlace.DTO.Auth;

namespace MarketFlow.Interface;

public interface IAuthService
{
    Task<DefaultResponse<string>> Login(AuthLoginDTO dto);
    Task<DefaultResponse<string>> SignUp(AuthRegisterDTO dto);
}