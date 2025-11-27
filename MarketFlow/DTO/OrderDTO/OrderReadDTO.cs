namespace MarketPlace.DTO;

public class OrderReadDTO
{
    public int Id { get; set; }
    public string UserFullName { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
    public string? PaymentType { get; set; }
    public string? Address { get; set; }
}