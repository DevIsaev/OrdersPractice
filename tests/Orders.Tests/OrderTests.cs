using Orders.Models;

namespace Orders.Tests;

public class OrderTests
{
    // Готовый пример теста. Остальные тесты вы пишете по аналогии.
    [Fact]
    public void Total_TwoItems_ReturnsSum()
    {
        // Arrange
        var order = new Order();
        order.Add("Книга", 300m, 2);
        order.Add("Ручка", 50m, 1);

        // Act
        var total = order.Total();

        // Assert
        Assert.Equal(650m, total);
    }

    [Fact]
    public void Total_EmptyOrder_ReturnsZero()
    {
        // Arrange
        var order = new Order();

        // Act
        var total = order.Total();

        // Assert
        Assert.Equal(0m, total);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Add_InvalidName_Throws(string? name)
    {
        // Arrange
        var order = new Order();

        // Act + Assert
        Assert.Throws<ArgumentException>(() => order.Add(name!, 100m, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Add_NonPositivePrice_Throws(decimal price)
    {
        // Arrange
        var order = new Order();

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => order.Add("Книга", price, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Add_InvalidQuantity_Throws(int quantity)
    {
        // Arrange
        var order = new Order();

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => order.Add("Книга", 100m, quantity));
    }

    [Fact]
    public void Add_ValidItem_AddsToItems()
    {
        // Arrange
        var order = new Order();

        // Act
        order.Add("Книга", 300m, 2);

        // Assert
        var item = Assert.Single(order.Items);
        Assert.Equal("Книга", item.Name);
        Assert.Equal(300m, item.Price);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public void Constructor_WithItems_SameTotalAsAdd()
    {
        // Arrange
        var items = new List<OrderItem>
        {
            new("Книга", 300m, 2),
            new("Ручка", 50m, 1)
        };

        // Act
        var order = new Order(items);

        // Assert
        Assert.Equal(650m, order.Total());
    }
}
