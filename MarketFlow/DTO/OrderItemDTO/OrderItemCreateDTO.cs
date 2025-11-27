namespace MarketPlace.DTO.OrderItemDTO;

public class OrderItemCreateDTO
{
    public int OrderId { get; set; }
    
    public int ProductId { get; set; }
    
    public int Quantity { get; set; }
}