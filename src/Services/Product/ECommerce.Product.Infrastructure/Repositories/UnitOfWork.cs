using ECommerce.Product.Domain.Interfaces;
using ECommerce.Product.Infrastructure.Data;
using MongoDB.Driver;

namespace ECommerce.Product.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MongoDbContext _context;
    private readonly IMongoClient _mongoClient;
    private IClientSessionHandle? _session;

    public UnitOfWork(MongoDbContext context, IMongoClient mongoClient)
    {
        _context = context;
        _mongoClient = mongoClient;
        Products = new ProductRepository(_context);
        Categories = new CategoryRepository(_context);
        Tags = new TagRepository(_context);
    }

    public IProductRepository Products { get; }
    public ICategoryRepository Categories { get; }
    public ITagRepository Tags { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // MongoDB doesn't have a traditional SaveChanges
        // Changes are committed immediately with each operation
        // This method is kept for interface compatibility
        return await Task.FromResult(0);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _session = await _mongoClient.StartSessionAsync(cancellationToken: cancellationToken);
        _session.StartTransaction();
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_session != null)
        {
            await _session.CommitTransactionAsync(cancellationToken);
            _session.Dispose();
            _session = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_session != null)
        {
            await _session.AbortTransactionAsync(cancellationToken);
            _session.Dispose();
            _session = null;
        }
    }

    public void Dispose()
    {
        _session?.Dispose();
        GC.SuppressFinalize(this);
    }
}
