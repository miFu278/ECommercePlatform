using ECommerce.Shared.Abstractions.Entities;

namespace ECommerce.User.Domain.Entities;

public class Address : BaseEntity
{
    public Guid UserId { get; set; }
    public string AddressType { get; set; } = "shipping"; // shipping, billing
    public string StreetAddress { get; set; } = string.Empty;
    public string? Apartment { get; set; }
    public string City { get; set; } = string.Empty;
    public string? StateProvince { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    
    // Computed property
    public string FullAddress => 
        $"{StreetAddress}{(string.IsNullOrEmpty(Apartment) ? "" : ", " + Apartment)}, {City}, {StateProvince} {PostalCode}, {Country}";
}
