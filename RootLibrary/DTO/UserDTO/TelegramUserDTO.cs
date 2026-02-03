using System.Runtime;

namespace RootLibrary.DTO.UserDTO
{
    public class TelegramUserDTO
    {
        public int userId { get; set; }
        public long TelegramId { get; set; }   
        public string Username { get; set; }   
        public string? PhoneNumber { get; set; } 
        
        public string Role { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}