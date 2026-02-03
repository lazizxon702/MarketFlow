namespace RootLibrary.Models.Inner;

public class Category
{
    public int Id { get; set; }

    public string Keyword { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool IsDeleted { get; set; } = false;
    
  
    public int? mainCategoryId { get; set; }

    public ICollection<Product> Products { get; set; }
   
}