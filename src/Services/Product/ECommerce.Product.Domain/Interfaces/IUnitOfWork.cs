using ECommerce.Shared.Abstractions.Repositories;

namespace ECommerce.Product.Domain.Interfaces;

public interface IUnitOfWork : ECommerce.Shared.Abstractions.Repositories.IUnitOfWork
{
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    ITagRepository Tags { get; }
}
