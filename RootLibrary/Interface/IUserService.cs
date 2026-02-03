using RootLibrary.DTO.UserDTO;
using RootLibrary.Models.Response;

namespace RootLibrary.Interface
{
    public interface IUserService
    {
        Task<DefaultResponse<List<UserReadDTO>>> GetAll();
        
        Task<DefaultResponse<UserReadDTO>> GetById(int id);
        Task<DefaultResponse<TelegramUserDTO>> GetByTelegramId(long Id);
        
        
        Task<DefaultResponse<string>> CreateUser(TelegramUserDTO dto);
      
        Task<DefaultResponse<bool>> UpdateUser(int id, TelegramUserDTO dto);
        
        Task<DefaultResponse<bool>> Delete(int id);
        
    }
}