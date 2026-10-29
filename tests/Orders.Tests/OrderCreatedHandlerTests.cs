using Moq;
using Orders.Abstractions;
using Orders.Handlers;
using Orders.Models;

namespace Orders.Tests;

public class OrderCreatedHandlerTests
{
    readonly Mock<INotificationSender> _sender = new();
    readonly OrderCreatedHandler _sut;

    public OrderCreatedHandlerTests() => _sut = new(_sender.Object);

    [Fact]
    public async Task HandleAsync_ValidMessage_SendsNotification()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        await _sut.HandleAsync(new OrderCreated(id));

        // Assert
        _sender.Verify(s => s.SendAsync(It.Is<string>(t => t.Contains(id.ToString()))), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_EmptyOrderId_Throws()
    {
        // Arrange
        var message = new OrderCreated(Guid.Empty);

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.HandleAsync(message));

        // Assert
        Assert.Contains("OrderId", ex.Message);
        _sender.Verify(s => s.SendAsync(It.IsAny<string>()), Times.Never());
    }

    [Fact]
    public async Task HandleAsync_NullMessage_Throws()
    {
        // Act + Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.HandleAsync(null!));
    }
}

