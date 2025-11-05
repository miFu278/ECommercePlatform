namespace ECommerce.User.Application.DTOs;

public record AddressDto(
    Guid Id,
    string AddressType,
    string StreetAddress,
    string? Apartment,
    string City,
    string? StateProvince,
    string PostalCode,
    string Country,
    bool IsDefault,
    string FullAddress,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateAddressDto(
    string AddressType,
    string StreetAddress,
    string? Apartment,
    string City,
    string? StateProvince,
    string PostalCode,
    string Country,
    bool IsDefault = false
);

public record UpdateAddressDto(
    string AddressType,
    string StreetAddress,
    string? Apartment,
    string City,
    string? StateProvince,
    string PostalCode,
    string Country,
    bool IsDefault
);
