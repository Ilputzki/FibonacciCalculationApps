using HttpReceiverApp;
using Processing;
using Sending;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices<long>(builder.Services, builder.Configuration);

var app = builder.Build();
app.UseHttpsRedirection();

MapRoutes();

app.Run();

void ConfigureServices<T>(IServiceCollection services, IConfiguration configuration)
{
    ConfigureSettings<T>(services);
    ConfigureCalculationUtils(services);

    services.AddSingleton<IConnection<EasyNetQ.IBus>, EasyNetQConnection>(s => { return new EasyNetQConnection(configuration.GetConnectionString("RabbitMQ")); });
    services.AddSingleton<IProcessor<long>, FibonacciProcessor<long>>();
    services.AddTransient<ISender<long>, EasyNetQSender<long>>();
    services.AddSingleton<RequestsHandlingService<long>>();
}

void ConfigureSettings<T>(IServiceCollection services)
{
    services.Configure<FibonacciCalculationSettings<long>>(builder.Configuration.GetSection("FibonacciCalculationSettings"));
}

void ConfigureCalculationUtils(IServiceCollection services)
{
    services.AddTransient<ICalculator<long>, LongCalculator>();
    services.AddTransient<IComparer<long>, LongComparer>();
}

void MapRoutes()
{
    app.MapPost("/fibonacci", async (Message<long> message) =>
    {
        var handlingService = app.Services.GetRequiredService<RequestsHandlingService<long>>();
        return await handlingService.ProcessReceivedMessageAsync(message).ContinueWith(task =>
        {
            if (task.Exception != null)
                return Results.StatusCode(500);
            return Results.Ok();
        });
    });
}