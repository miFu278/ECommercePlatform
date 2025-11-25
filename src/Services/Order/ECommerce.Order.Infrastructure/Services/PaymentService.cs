using System.Text;
using System.Text.Json;
using ECommerce.Order.Application.DTOs;
using ECommerce.Order.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.Order.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentService> _logger;
    private readonly string _paymentServiceUrl;

    public PaymentService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<PaymentService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _paymentServiceUrl = configuration["Services:Payment"] ?? "http://localhost:5004";
        _httpClient.BaseAddress = new Uri(_paymentServiceUrl);
    }

    public async Task<CreatePaymentResponseDto> CreatePaymentAsync(CreatePaymentRequestDto request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/payments", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CreatePaymentResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                throw new InvalidOperationException("Failed to deserialize payment response");
            }

            _logger.LogInformation("Payment created successfully for Order: {OrderNumber}", request.OrderNumber);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create payment for Order: {OrderNumber}", request.OrderNumber);
            throw;
        }
    }
}
