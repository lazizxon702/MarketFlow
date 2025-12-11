using MarketFlow.DTO.UserDTO;
using MarketFlow.Models.Response;


namespace MarketFlow.Interface
{
    public interface IUserService
    {
        Task<DefaultResponse<List<UserReadDTO>>> GetAll();
        
        Task<DefaultResponse<UserReadDTO>> GetById(int id);
        
        Task<DefaultResponse<string>> CreateUser(UserCreateDTO dto);
      
        Task<DefaultResponse<bool>> UpdateUser(int id, UserUpdateDTO dto);
        
        Task<DefaultResponse<bool>> Delete(int id);
        
    }
}