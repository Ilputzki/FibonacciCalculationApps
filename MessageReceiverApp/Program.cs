using Processing;
using Sending;
using MessageReceiverApp;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices<long>(builder.Services, builder.Configuration);

var app = builder.Build();
app.UseHttpsRedirection();

app.Run();

void ConfigureServices<T>(IServiceCollection services, IConfiguration configuration)
{
    ConfigureSettings<T>(services);
    ConfigureCalculationUtils(services);

    services.AddSingleton<IConnection<EasyNetQ.IBus>, EasyNetQConnection>(s => { return new EasyNetQConnection(configuration.GetConnectionString("RabbitMQ")); });
    services.AddSingleton<IConnection<HttpClient>, HttpConnection>(s => { return new HttpConnection(configuration.GetConnectionString("CalculatorServiceUri")); });
    services.AddSingleton<IProcessor<T>, FibonacciProcessor<T>>();
    services.AddTransient<ISender<T>, HttpSender<T>>();
    services.AddHostedService<MessagesHandlingService<T>>();
}

void ConfigureSettings<T>(IServiceCollection services)
{
    services.Configure<FibonacciCalculationSettings<T>>(builder.Configuration.GetSection("FibonacciCalculationSettings"));
    services.Configure<MessagesHandlingServiceSettings<T>>(builder.Configuration.GetSection("MessagesHandlingServiceSettings"));
}

void ConfigureCalculationUtils(IServiceCollection services)
{
    services.AddTransient<ICalculator<long>, LongCalculator>();
    services.AddTransient<IComparer<long>, LongComparer>();
}