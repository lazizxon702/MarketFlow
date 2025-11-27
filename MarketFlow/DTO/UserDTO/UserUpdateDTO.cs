namespace MarketPlace.DTO;

public class UserUpdateDTO
{
    public int Id { get; set; }
    
    public string FullName { get; set; }
   
    public string? Email { get; set; }
    
    public string Password { get; set; }
    
    public string Role { get; set; }
}