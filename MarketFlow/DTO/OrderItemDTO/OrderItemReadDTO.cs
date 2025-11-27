namespace MarketPlace.DTO.OrderItemDTO;

public class OrderItemReadDTO
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}