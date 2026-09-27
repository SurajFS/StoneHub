using Catalog.Application.Products;
using SharedKernel;
using Xunit;

namespace Catalog.Tests;

public sealed class DeleteProductCommandHandlerTests
{
    private static readonly Guid Owner = Guid.NewGuid();

    [Fact]
    public async Task Owner_DeletesListing()
    {
        var product = ProductTestData.Listing(Owner);
        var repository = new FakeProductRepository(product);

        var result = await new DeleteProductCommandHandler(repository)
            .Handle(new DeleteProductCommand(product.Id, Owner), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(product.DeletedAt);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task OtherSeller_IsForbidden_AndNothingChanges()
    {
        var product = ProductTestData.Listing(Owner);
        var repository = new FakeProductRepository(product);

        var result = await new DeleteProductCommandHandler(repository)
            .Handle(new DeleteProductCommand(product.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
        Assert.Null(product.DeletedAt);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task MissingListing_IsNotFound()
    {
        var repository = new FakeProductRepository();

        var result = await new DeleteProductCommandHandler(repository)
            .Handle(new DeleteProductCommand(Guid.NewGuid(), Owner), CancellationToken.None);

        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }
}
