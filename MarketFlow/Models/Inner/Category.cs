namespace MarketPlace;

public class Category
{
    public int Id { get; set; }
    
    public string Keyword { get; set; }
    
    public DateTime CreatedDate { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public ICollection<Product> Products { get; set; }
}



