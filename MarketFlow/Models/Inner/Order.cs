using MarketPlace.Enums;

namespace MarketPlace;

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public DateTime? OrderDate { get; set; } = DateTime.Now;

    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public PaymentType PaymentType { get; set; } = PaymentType.Cash;

    public string? Address { get; set; }

    public bool IsDeleted { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; }
}