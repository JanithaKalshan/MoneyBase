using Microsoft.Extensions.DependencyInjection;
using MoneyBase.Application.Services.Interfaces;
using MoneyBase.Infrastructure.Services;
using MoneyBase.Infrastructure.Utils;

namespace MoneyBase.Test.Fixtures;

public class ServiceFixture
{
    public ServiceProvider ServiceProvider { get; private set; }

    public ServiceFixture()
    {
        var service = new ServiceCollection();

        service.AddSingleton<IChatQueueService, ChatQueueService>();
        service.AddSingleton<TeamsCreator>();

        ServiceProvider = service.BuildServiceProvider();
    }

    public void Dispose()
    {
        ServiceProvider.Dispose();
    }
}
