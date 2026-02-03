namespace RootLibrary.DTO.UserDTO;

public class UserReadDTO
{
    public int Id { get; set; }
    
    public string Username { get; set; }
    public string PhoneNumber { get; set; } 
    
    public string Role { get; set; }
    
    public DateTime CreatedDate { get; set; }
}