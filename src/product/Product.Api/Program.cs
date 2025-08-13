using OpenTelemetry.Trace;
using System.Diagnostics;
using Confluent.Kafka.Extensions.OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);
builder.Logging
    .AddOpenTelemetry(options => options.AddOtlpExporter())
    .AddConsole();
// builder.Logging.ClearProviders();
// builder.Logging.AddOpenTelemetry(logging =>
// {
//     logging.IncludeFormattedMessage = true;
//     logging.IncludeScopes = true;
//     logging.ParseStateValues = true;
//
//     // logging.SetResourceBuilder(resourceBuilder);
//
//     logging.AddOtlpExporter(o =>
//     {
//         o.Endpoint = new Uri("http://otel-collector:4317"); // match your config
//     });
// });

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<Consumer>();

void AppResourceBuilder(ResourceBuilder resource) => resource.AddService(builder.Environment.ApplicationName);
// .AddContainerDetector()
        // .AddHostDetector();
builder.Services.AddOpenTelemetry()
    .ConfigureResource(AppResourceBuilder)
    .WithTracing(b =>
    {
      b.AddSource(ServiceName.Products)
      .AddHttpClientInstrumentation()
      .AddAspNetCoreInstrumentation()
      .AddConfluentKafkaInstrumentation()
      .AddOtlpExporter();
    });
        
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

var consumer = app.Services.GetRequiredService<Consumer>();
_ = Task.Run(() => consumer.StartListening());

app.MapGet("/api/products", () =>
    {
        
        var activity = Activity.Current;
        activity?.SetTag("app.user.id", "test");
        activity?.AddEvent(new("Fetch products"));
        // activity?.SetTag("app.product.id", request.Item.ProductId);
        // activity?.SetTag("app.product.quantity", request.Item.Quantity);v
        Product[] products = [new(1, "p1"), new Product(2, "p2")];
        return products;
    })
    .WithName("GetProducts")
    .WithOpenApi();

app.Run();

public record Product(long Id, string Name);