namespace ECommerce.ShoppingCart.Domain.Entities;

public class ShoppingCart
{
    public Guid UserId { get; set; }
    public List<CartItem> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Computed properties
    public int TotalItems => Items.Sum(x => x.Quantity);
    public decimal Subtotal => Items.Sum(x => x.Subtotal);
    public decimal TotalDiscount => Items.Sum(x => x.DiscountAmount * x.Quantity);
    public decimal Total => Subtotal - TotalDiscount;

    public void AddItem(CartItem item)
    {
        var existingItem = Items.FirstOrDefault(x => x.ProductId == item.ProductId);
        
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
            existingItem.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            Items.Add(item);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var item = Items.FirstOrDefault(x => x.ProductId == productId);
        if (item == null)
            throw new InvalidOperationException("Item not found in cart");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0");

        item.Quantity = quantity;
        item.UpdatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid productId)
    {
        var item = Items.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
        {
            Items.Remove(item);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Clear()
    {
        Items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}
