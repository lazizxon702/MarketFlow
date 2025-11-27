namespace MarketPlace.DTO.ProductDTO;

public class ProductReadDTO
{
    public int Id { get; set; }
    
    public string NameS { get; set; }
    
    public string Description { get; set; }
    
    public decimal Price { get; set; }
    
    public string CategoryName { get; set; }
    public DateTime CreatedDate { get; set; }
}