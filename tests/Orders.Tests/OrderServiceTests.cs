using Moq;
using Orders.Abstractions;
using Orders.Models;
using Orders.Services;

namespace Orders.Tests
{
    public class OrderServiceTests
    {
        readonly Mock<IOrderRepository> _repo = new();
        readonly Mock<IMessagePublisher> _pub = new();
        readonly OrderService _sut;

        public OrderServiceTests() =>
        _sut = new(_repo.Object, _pub.Object, new DiscountCalculator());

        static NewOrder Cmd() =>
            new(new List<OrderItem> { new("Книга", 300m, 2) });

        static Order OrderWithTotal(decimal total)
        {
            var order = new Order();
            order.Add("Товар", total, 1);
            return order;
        }

        [Fact]
        public async Task CreateAsync_ValidOrder_SavesOrder()
        {
            // Act
            await _sut.CreateAsync(Cmd());

            // Assert
            _repo.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Once());
        }

        [Fact]
        public async Task CreateAsync_ValidOrder_PublishesEventWithSameId()
        {
            // Act
            var id = await _sut.CreateAsync(Cmd());

            // Assert
            _pub.Verify(p => p.PublishAsync(It.IsAny<OrderCreated>()), Times.Once());
            _pub.Verify(p => p.PublishAsync(It.Is<OrderCreated>(e => e.OrderId == id)), Times.Once());
        }

        [Fact]
        public async Task CreateAsync_SaveFails_DoesNotPublish()
        {
            // Arrange
            _repo.Setup(r => r.SaveAsync(It.IsAny<Order>()))
                 .ThrowsAsync(new TimeoutException());

            // Act
            await Assert.ThrowsAsync<TimeoutException>(() => _sut.CreateAsync(Cmd()));

            // Assert
            _pub.Verify(p => p.PublishAsync(It.IsAny<OrderCreated>()), Times.Never());
        }

        [Fact]
        public async Task CreateAsync_NullCommand_Throws()
        {
            // Act + Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_EmptyItems_ThrowsAndDoesNotSave()
        {
            // Arrange
            var cmd = new NewOrder(new List<OrderItem>());

            // Act
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(cmd));

            // Assert
            _repo.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Never());
        }

        [Fact]
        public async Task CreateAsync_PublishFails_ExceptionPropagates()
        {
            // Arrange
            _pub.Setup(p => p.PublishAsync(It.IsAny<OrderCreated>()))
                .ThrowsAsync(new InvalidOperationException("broker down"));

            // Act + Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(Cmd()));
            Assert.Equal("broker down", ex.Message);
        }

        [Fact]
        public async Task GetPayableAsync_TotalBelowThreshold_NoDiscount()
        {
            // Arrange
            var order = OrderWithTotal(500m);
            _repo.Setup(r => r.FindAsync(order.Id)).ReturnsAsync(order);

            // Act
            var payable = await _sut.GetPayableAsync(order.Id);

            // Assert
            Assert.Equal(500m, payable);
        }

        [Fact]
        public async Task GetPayableAsync_TotalAboveThreshold_AppliesDiscount()
        {
            // Arrange
            var order = OrderWithTotal(1500m);
            _repo.Setup(r => r.FindAsync(order.Id)).ReturnsAsync(order);

            // Act
            var payable = await _sut.GetPayableAsync(order.Id);

            // Assert
            Assert.Equal(1350m, payable);
        }

        [Fact]
        public async Task GetPayableAsync_OrderNotFound_Throws()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repo.Setup(r => r.FindAsync(id)).ReturnsAsync((Order?)null);

            // Act + Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetPayableAsync(id));
        }

    }
}
