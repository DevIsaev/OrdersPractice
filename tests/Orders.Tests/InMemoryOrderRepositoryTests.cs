using Orders.Models;
using Orders.Repositories;

namespace Orders.Tests;

public class InMemoryOrderRepositoryTests
{
    private readonly InMemoryOrderRepository _sut = new();

    [Fact]
    public async Task SaveAsync_ThenFindAsync_ReturnsSameOrder()
    {
        // Arrange
        var order = new Order();
        order.Add("Книга", 300m, 2);

        // Act
        await _sut.SaveAsync(order);
        var found = await _sut.FindAsync(order.Id);

        // Assert
        Assert.Same(order, found);
    }

    [Fact]
    public async Task FindAsync_UnknownId_ReturnsNull()
    {
        // Act
        var found = await _sut.FindAsync(Guid.NewGuid());

        // Assert
        Assert.Null(found);
    }

    [Fact]
    public async Task SaveAsync_Null_Throws()
    {
        // Act + Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.SaveAsync(null!));
    }
}
