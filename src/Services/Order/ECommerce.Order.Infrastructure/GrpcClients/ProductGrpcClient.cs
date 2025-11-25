using ECommerce.Product.Grpc;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.Order.Infrastructure.GrpcClients;

public interface IProductGrpcClient
{
    Task<ProductInfoResponse> GetProductInfoAsync(string productId);
    Task<StockCheckResponse> CheckStockAsync(string productId, int quantity);
    Task<ValidateProductsResponse> ValidateProductsAsync(List<(string ProductId, int Quantity)> items);
}

public class ProductGrpcClient : IProductGrpcClient
{
    private readonly ProductGrpcService.ProductGrpcServiceClient _client;
    private readonly ILogger<ProductGrpcClient> _logger;

    public ProductGrpcClient(IConfiguration configuration, ILogger<ProductGrpcClient> logger)
    {
        _logger = logger;
        
        var grpcUrl = configuration["Services:Product:GrpcUrl"] ?? "http://localhost:5011";
        var channel = GrpcChannel.ForAddress(grpcUrl);
        _client = new ProductGrpcService.ProductGrpcServiceClient(channel);
    }

    public async Task<ProductInfoResponse> GetProductInfoAsync(string productId)
    {
        try
        {
            var request = new ProductInfoRequest { ProductId = productId };
            return await _client.GetProductInfoAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get product info for {ProductId}", productId);
            throw;
        }
    }

    public async Task<StockCheckResponse> CheckStockAsync(string productId, int quantity)
    {
        try
        {
            var request = new StockCheckRequest 
            { 
                ProductId = productId, 
                Quantity = quantity 
            };
            return await _client.CheckStockAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check stock for {ProductId}", productId);
            throw;
        }
    }

    public async Task<ValidateProductsResponse> ValidateProductsAsync(List<(string ProductId, int Quantity)> items)
    {
        try
        {
            var request = new ValidateProductsRequest();
            foreach (var item in items)
            {
                request.Items.Add(new ProductValidationItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                });
            }
            return await _client.ValidateProductsAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate products");
            throw;
        }
    }
}
