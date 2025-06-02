using Microsoft.Extensions.DependencyInjection;
using MoneyBase.Application.Handlers.Command;
using MoneyBase.Application.Services.Interfaces;
using MoneyBase.Domain.Entities;
using MoneyBase.Test.Common;
using System.Net.Http.Json;

namespace MoneyBase.Test.API;

public class ChatControllerTest : IClassFixture<CustomizeWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomizeWebApplicationFactory _factory;
    public ChatControllerTest(CustomizeWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task StartChatShouldReturnChatId()
    {
        // Arrange
        var command = new CreateChatCommand("testUser123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Chat/start-chat", command);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task StartChat_ShouldReturnBadQueueIsFullMessage_WhenQueueIsFull()
    {
        // Arrange
        var command = new CreateChatCommand("testUserQueueFull");

        using (var scope = _factory.Services.CreateScope())
        {
            var chatQueueService = scope.ServiceProvider.GetRequiredService<IChatQueueService>();
            for (int i = 0; i < 1000; i++)
            {
                chatQueueService.AddToQueue(Guid.NewGuid(), $"fillerUser{i}");
            }
        }

        // Act
        var response = await _client.PostAsJsonAsync("/api/Chat/start-chat", command);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains(Guid.Empty.ToString(), errorContent);

    }

    [Fact]
    public async Task PollChat_ShouldReturnOk_AndUpdateTime()
    {
        // Arrange

        var startCommand = new CreateChatCommand("UserPolling");
        var startResponse = await _client.PostAsJsonAsync("/api/Chat/start-chat", startCommand);
        startResponse.EnsureSuccessStatusCode();
        var startResult = await startResponse.Content.ReadFromJsonAsync<ChatResponse>();
        var chatId = startResult!.ChatId;


        DateTime initialPollTime;
        using (var scope = _factory.Services.CreateScope())
        {
            var chatQueueService = scope.ServiceProvider.GetRequiredService<IChatQueueService>();
            var chat = chatQueueService.GetChatById(chatId);
            Assert.NotNull(chat);
            initialPollTime = chat.LastPolledAt;
        }

        await Task.Delay(100);

        var pollCommand = new ChatPollCommand(chatId);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Chat/poll-chat", pollCommand);

        // Assert
        response.EnsureSuccessStatusCode(); 
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);


        using (var scope = _factory.Services.CreateScope())
        {
            var chatQueueService = scope.ServiceProvider.GetRequiredService<IChatQueueService>();
            var chat = chatQueueService.GetChatById(chatId);
            Assert.NotNull(chat);
            Assert.True(chat.LastPolledAt > initialPollTime);
            Assert.Equal(0, chat.MissedPolls);
        }
    }

    [Fact]
    public async Task GetAllChats_ShouldReturnListOfChats()
    {
        // Arrange

        using (var scope = _factory.Services.CreateScope())
        {
            var chatQueueService = scope.ServiceProvider.GetRequiredService<IChatQueueService>();


            chatQueueService.AddToQueue(Guid.NewGuid(), "User1");
            chatQueueService.AddToQueue(Guid.NewGuid(), "User2");

        }

        // Act
        var response = await _client.GetAsync("/api/Chat/get-all-chats");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<ChatSession>>();
        Assert.NotNull(result);
        Assert.True(result.Count >= 2); 
        Assert.All(result, c => Assert.True(c.IsActive));
    }

}

