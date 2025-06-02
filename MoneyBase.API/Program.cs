using MoneyBase.Application;
using MoneyBase.Application.Services.Interfaces;
using MoneyBase.Infrastructure.BackgroundServices;
using MoneyBase.Infrastructure.Services;
using MoneyBase.Infrastructure.Utils;

namespace MoneyBase.API;
public partial class Program {

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddSwaggerGen();
        builder.Services.AddApplicationServices();
        builder.Services.AddLogging();
        builder.Services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations(); 
           
        });



        builder.Services.AddSingleton<IChatQueueService, ChatQueueService>();
        builder.Services.AddSingleton<TeamsCreator>();

        //Register background services
        builder.Services.AddHostedService<ChatAgentAllocateService>();
        builder.Services.AddHostedService<PollingService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "MoneyBase API v1");
                options.RoutePrefix = string.Empty;
            });
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
