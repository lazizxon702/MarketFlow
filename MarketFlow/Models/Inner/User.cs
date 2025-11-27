namespace MarketPlace;

public class User
{
    public int Id { get; set; }
    
    public string Username { get; set; }
    
    public string? Email { get; set; }
    
    public string Password { get; set; }
    
    public string Role { get; set; } = "User";
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    
    public bool IsDeleted { get; set; } = false;
    
    public ICollection<Order> Orders { get; set; }
}