namespace RootLibrary.DTO.UserDTO;

public class UserUpdateDTO
{
    public int Id { get; set; }
    public long TelegramId { get; set; }
    
    public string FullName { get; set; }
    
    public string PhoneNumber { get; set; } 
    
    
    public string Role { get; set; }
}