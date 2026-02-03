namespace RootLibrary.Models.Inner;

public class Cart
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int Quantity { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public bool IsOrdered { get; set; } = false;
    
    public ICollection<OrderItem> OrderItems { get; set; }

}
