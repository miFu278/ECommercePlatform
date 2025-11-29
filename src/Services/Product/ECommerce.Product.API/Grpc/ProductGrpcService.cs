using ECommerce.Product.Application.Interfaces;
using ECommerce.Product.Domain.Enums;
using ECommerce.Product.Grpc;
using Grpc.Core;

namespace ECommerce.Product.API.Grpc;

public class ProductGrpcService : Product.Grpc.ProductGrpcService.ProductGrpcServiceBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductGrpcService> _logger;

    public ProductGrpcService(IProductService productService, ILogger<ProductGrpcService> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public override async Task<ProductInfoResponse> GetProductInfo(ProductInfoRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetProductInfo called for ProductId: {ProductId}", request.ProductId);

        var product = await _productService.GetByIdAsync(request.ProductId);

        if (product == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product {request.ProductId} not found"));
        }

        return new ProductInfoResponse
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            Price = (double)product.Price,
            StockQuantity = product.Stock,
            IsAvailable = product.Status == ProductStatus.Active && product.IsPublished && product.Stock > 0,
            ImageUrl = product.Images?.FirstOrDefault()?.Url ?? "",
            Currency = "VND"
        };
    }

    public override async Task<ProductsBatchResponse> GetProductsBatch(ProductsBatchRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetProductsBatch called for {Count} products", request.ProductIds.Count);

        var response = new ProductsBatchResponse();

        foreach (var productId in request.ProductIds)
        {
            var product = await _productService.GetByIdAsync(productId);
            if (product != null)
            {
                response.Products.Add(new ProductInfoResponse
                {
                    Id = product.Id,
                    Name = product.Name,
                    Sku = product.Sku,
                    Price = (double)product.Price,
                    StockQuantity = product.Stock,
                    IsAvailable = product.Status == ProductStatus.Active && product.IsPublished && product.Stock > 0,
                    ImageUrl = product.Images?.FirstOrDefault()?.Url ?? "",
                    Currency = "VND"
                });
            }
        }

        return response;
    }


    public override async Task<StockCheckResponse> CheckStock(StockCheckRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC CheckStock called for ProductId: {ProductId}, Quantity: {Quantity}",
            request.ProductId, request.Quantity);

        var product = await _productService.GetByIdAsync(request.ProductId);

        if (product == null)
        {
            return new StockCheckResponse
            {
                Available = false,
                StockQuantity = 0,
                Message = "Product not found"
            };
        }

        var isAvailable = product.Status == ProductStatus.Active && product.IsPublished && product.Stock > 0;
        var available = product.Stock >= request.Quantity && isAvailable;

        return new StockCheckResponse
        {
            Available = available,
            StockQuantity = product.Stock,
            Message = available ? "Stock available" : $"Only {product.Stock} items available"
        };
    }

    public override async Task<ValidateProductsResponse> ValidateProducts(ValidateProductsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC ValidateProducts called for {Count} products", request.Items.Count);

        var response = new ValidateProductsResponse
        {
            AllValid = true
        };

        foreach (var item in request.Items)
        {
            var product = await _productService.GetByIdAsync(item.ProductId);

            var result = new ProductValidationResult
            {
                ProductId = item.ProductId
            };

            if (product == null)
            {
                result.Valid = false;
                result.Message = "Product not found";
                result.AvailableQuantity = 0;
                response.AllValid = false;
            }
            else if (product.Status != ProductStatus.Active || !product.IsPublished)
            {
                result.Valid = false;
                result.Message = "Product not available";
                result.AvailableQuantity = 0;
                response.AllValid = false;
            }
            else if (product.Stock < item.Quantity)
            {
                result.Valid = false;
                result.Message = $"Insufficient stock. Only {product.Stock} available";
                result.AvailableQuantity = product.Stock;
                response.AllValid = false;
            }
            else
            {
                result.Valid = true;
                result.Message = "Valid";
                result.AvailableQuantity = product.Stock;
            }

            response.Results.Add(result);
        }

        return response;
    }
}
