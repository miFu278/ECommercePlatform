namespace ECommerce.User.Domain.Enums;

public enum RoleType
{
    Admin = 1,      // Full system access
    Manager = 2,    // Product and order management
    Customer = 3,   // Standard customer access
    Guest = 4       // Limited browsing access
}
