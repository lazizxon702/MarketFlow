namespace MarketFlow.Models.Inner;

public class Product
{
    public int Id { get; set; }
    
    public string NameS { get; set; }
    
    public string Description { get; set; }
    
    public decimal Price { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public int CategoryId { get; set; }
    
    public Category Category { get; set; }
    
    public bool IsDeleted { get; set; } 
    
    public ICollection<OrderItem> OrderItems{ get; set; }
}