using System.Threading.Tasks;
using LeaveMgmt.Infrastructure.Messaging;
using Moq;
using StackExchange.Redis;
using Xunit;

public class RedisEventBusTests
{
    [Fact] // Unit
    public async Task PublishAsync_Should_Forward_To_Redis()
    {
        // Arrange
        var db = new Mock<IDatabase>();
        db.Setup(d => d.PublishAsync(It.IsAny<RedisChannel>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
          .ReturnsAsync(1);

        var mux = new Mock<IConnectionMultiplexer>();
        mux.Setup(m => m.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);

        var bus = new RedisEventBus(mux.Object);

        // Act
        await bus.PublishAsync("LeaveRequestApproved", new { Id = "abc" });

        // Assert
        db.Verify(d => d.PublishAsync(
            It.Is<RedisChannel>(c => c.ToString() == "LeaveRequestApproved"),
            It.IsAny<RedisValue>(),
            It.IsAny<CommandFlags>()), Times.Once);
    }
}
