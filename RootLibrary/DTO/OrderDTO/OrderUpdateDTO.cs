namespace RootLibrary.DTO.OrderDTO;

public class OrderUpdateDTO
{
    public decimal TotalAmount { get; set; }
    
    public string Status { get; set; }
    
    public string PaymentType { get; set; }
    
    public string Address { get; set; }
}
