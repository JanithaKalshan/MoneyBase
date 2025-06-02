using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using MoneyBase.API;
using MoneyBase.Application.Services.Interfaces;
using MoneyBase.Infrastructure.Services;
using MoneyBase.Infrastructure.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace MoneyBase.Test;

public class CustomizeWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IChatQueueService, ChatQueueService>();
            services.AddSingleton<TeamsCreator>();
        });
    }
}
