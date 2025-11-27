using MarketPlace.DTO.Auth;
using MarketPlace.DTO.AuthDTO;
using MarketPlace.Models.Response;

namespace MarketPlace.Interface;

public interface IAuthService
{
    Task<DefaultResponse<string>> Login(AuthLoginDTO dto);
    Task<DefaultResponse<string>> SignUp(AuthRegisterDTO dto);
}