using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoneyBase.Application.Handlers.Command;
using MoneyBase.Application.Handlers.Query;
using MoneyBase.Application.Services.Interfaces;
using MoneyBase.Domain.Entities;
using MoneyBase.Infrastructure.Utils;
using MoneyBase.Test.Fixtures;
using Moq;

namespace MoneyBase.Test.Application;

public class ChatHandlerTests : IClassFixture<ServiceFixture>
{
    private readonly ServiceFixture _fixture;
    private readonly TeamsCreator _teamCreator;
    private readonly IChatQueueService _chatService;

    public ChatHandlerTests(ServiceFixture fixture)
    {
        _fixture = fixture;
        _teamCreator = _fixture.ServiceProvider.GetRequiredService<TeamsCreator>();
        _chatService = _fixture.ServiceProvider.GetRequiredService<IChatQueueService>();
    }

    [Fact]
    public async Task CreateChatHandlerShouldAddChatAndReturnChatId_WhenSuccessful()
    {
        // Arrange
        var mockChatQueueService = new Mock<IChatQueueService>();
        var mockLogger = new Mock<ILogger<CreateChatCommandHandler>>();

        mockChatQueueService.Setup(s => s.AddToQueue(It.IsAny<Guid>(), It.IsAny<string>()))
                            .Returns(true);

        var handler = new CreateChatCommandHandler(mockChatQueueService.Object, mockLogger.Object);
        var command = new CreateChatCommand("testUser123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        mockChatQueueService.Verify(s => s.AddToQueue(It.IsAny<Guid>(), command.UserId), Times.Once);
        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public void AddChatShouldReturnFalseWhenQueueIsFull()
    {
        // Arrange
        var maxQueueSize = _teamCreator.GetMaxQueueSize();
        for (int i = 0; i < 100; i++)
        {
            _chatService.AddToQueue(Guid.NewGuid(), $"user{i}");
        }

        var newChatId = Guid.NewGuid();
        var newUserId = "overflowUser";

        // Act
        var success = _chatService.AddToQueue(newChatId, newUserId);

        // Assert
        Assert.False(success);
        Assert.True(maxQueueSize < _chatService.ChatQueueCurrentSize());
    }

    [Fact]
    public void RemoveChatShouldRemoveChatFromQueueAndReturnIt()
    {
        // Arrange
        var chatId1 = Guid.NewGuid();
        var chatId2 = Guid.NewGuid();
        _chatService.AddToQueue(chatId1, "user1");
        _chatService.AddToQueue(chatId2, "user2");

        // Act
        var currentSize = _chatService.ChatQueueCurrentSize();
        var dequeuedChat = _chatService.RemoveFromQueue();

        // Assert
        Assert.NotNull(dequeuedChat);
        Assert.Equal(currentSize-1, _chatService.ChatQueueCurrentSize());
    }

    [Fact]
    public void RemoveChat_ShouldReturnNull_WhenQueueIsEmpty()
    {
        //Arrange
        ChatSession? dequeuedChat = null;
        var currentSize = _chatService.ChatQueueCurrentSize();
        // Act
        for (int i = 0; i <= currentSize; i++)
        {
             dequeuedChat = _chatService.RemoveFromQueue();
        }      

        // Assert
        Assert.Null(dequeuedChat);
    }

    #region GetActiveChatsQueryHandler Tests

    [Fact]
    public async Task GetAllChatsQueryHandlerShouldReturnListOfAllChats()
    {
        // Arrange
        var mockChatQueueService = new Mock<IChatQueueService>();
        var activeChat1 = new ChatSession { Id = Guid.NewGuid(), UserId = "userA", IsActive = true };
        var activeChat2 = new ChatSession { Id = Guid.NewGuid(), UserId = "userB", IsActive = true };
        var inactiveChat = new ChatSession { Id = Guid.NewGuid(), UserId = "userC", IsActive = false };


        var serviceReturns = new List<ChatSession> { activeChat1, activeChat2 };
        mockChatQueueService.Setup(s => s.GetAllChats())
                            .Returns(serviceReturns);

        var handler = new GetAllChatsQueryHandler(mockChatQueueService.Object);
        var query = new GetAllChatsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); 
        Assert.Contains(activeChat1, result);
        Assert.Contains(activeChat2, result);
        Assert.DoesNotContain(inactiveChat, result); 
    }

    [Fact]
    public async Task GetAllChatsQueryHandlerShouldReturnEmptyList_WhenNoChats()
    {
        // Arrange
        var mockChatQueueService = new Mock<IChatQueueService>();
        mockChatQueueService.Setup(s => s.GetAllChats())
                            .Returns(new List<ChatSession>());

        var handler = new GetAllChatsQueryHandler(mockChatQueueService.Object);
        var query = new GetAllChatsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result); 
    }
    #endregion

    #region PollingService Tests

    [Fact]
    public async Task ChatPollCommandHandlerShouldCallUpdateChatSessionPollOnService()
    {
        // Arrange
        var mockChatQueueService = new Mock<IChatQueueService>();
        var chatIdToPoll = Guid.NewGuid();

        mockChatQueueService.Setup(s => s.UpdateChatSessionPoll(It.IsAny<Guid>()));

      
        var handler = new ChatPollCommandHandler(mockChatQueueService.Object); 
        var command = new ChatPollCommand(chatIdToPoll);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert

        mockChatQueueService.Verify(s => s.UpdateChatSessionPoll(chatIdToPoll), Times.Once);

        Assert.Equal(Unit.Value, result);
    }

    #endregion
}
