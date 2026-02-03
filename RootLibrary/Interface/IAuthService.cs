using RootLibrary.DTO.AuthDTO;
using RootLibrary.Models.Response;

namespace RootLibrary.Interface;

public interface IAuthService
{
    Task<DefaultResponse<string>> Login(AuthLoginDTO dto);
    Task<DefaultResponse<string>> SignUp(AuthRegisterDTO dto);
}