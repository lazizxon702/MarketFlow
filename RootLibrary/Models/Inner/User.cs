namespace RootLibrary.Models.Inner;

public class User
{
    public int Id { get; set; }
    public long TelegramId { get; set; }
    
    public string Username { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public string Role { get; set; } = "User";
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    
    public bool IsDeleted { get; set; } = false;
    
    public ICollection<Order> Orders { get; set; }
    public ICollection<Cart> Carts { get; set; }
}