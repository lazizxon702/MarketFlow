using RootLibrary.DTO.ProductDTO;

namespace RootLibrary.DTO.CategoryDTO;

public class CategoryChildDTO
{
    public int Id { get; set; }
    public string Keyword { get; set; }
    
    public List<ProductReadDTO> Products { get; set; }
}