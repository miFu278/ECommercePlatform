using System.Text.Json;
using ECommerce.Order.Application.Interfaces;

namespace ECommerce.Order.Infrastructure.Services;

public class ShoppingCartService : IShoppingCartService
{
    private readonly HttpClient _httpClient;

    public ShoppingCartService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CartDto?> GetCartAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/cart?userId={userId}");
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CartDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    public async Task ClearCartAsync(Guid userId)
    {
        try
        {
            await _httpClient.DeleteAsync($"/api/cart?userId={userId}");
        }
        catch
        {
            // Ignore errors when clearing cart
        }
    }
}
