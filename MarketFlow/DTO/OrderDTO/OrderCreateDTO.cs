namespace MarketPlace.DTO;

public class OrderCreateDTO
{
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentType { get; set; }
    public string Address { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
}