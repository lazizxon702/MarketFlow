namespace MarketPlace.DTO.ProductDTO;

public class ProductCreateDTO
{
    public string NameS { get; set; }
    
    public string Description { get; set; }
    
    public decimal Price { get; set; }
    
    public int CategoryId { get; set; }
    
}