using MarketFlow.DTO.ProductDTO;

namespace MarketFlow.DTO.CategoryDTO;

public class CategoryReadDTO
{
    public int Id { get; set; }
    public string Keyword { get; set; }
    
    public List<ProductReadDTO> Products { get; set; }

}