using OpenTelemetry.Trace;
using System.Diagnostics;
using Confluent.Kafka.Extensions.OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);
builder.Logging
    .AddOpenTelemetry(options =>
    {
        options.AddOtlpExporter();
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
        options.ParseStateValues = true;
    })
    .AddConsole();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<Consumer>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(x => x.AddService(builder.Environment.ApplicationName))
    .WithTracing(b =>
    {
      b.AddSource(ServiceName.Products)
      .AddHttpClientInstrumentation()
      .AddAspNetCoreInstrumentation()
      .AddConfluentKafkaInstrumentation()
      .SetSampler(new ParentBasedSampler(new AlwaysOnSampler()))
      .AddOtlpExporter();
    }).WithMetrics(meterBuilder => meterBuilder
        // .AddMeter("Demo")
        // .AddMeter("OpenFeature")
        .AddProcessInstrumentation()
        .AddRuntimeInstrumentation()
        .AddAspNetCoreInstrumentation()
        .SetExemplarFilter(ExemplarFilterType.TraceBased)
        .AddOtlpExporter());
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

var consumer = app.Services.GetRequiredService<Consumer>();
_ = Task.Run(() => consumer.StartListening());

app.MapGet("/api/products", (ILogger<Program> logger) =>
    {
        
        var activity = Activity.Current;
        activity?.SetTag("app.user.id", "test");
        activity?.AddEvent(new("Fetch products"));
        Product[] products = [new(1, "p1"), new Product(2, "p2")];
        logger.LogInformation("Fetching products");
        return products;
    })
    .WithName("GetProducts")
    .WithOpenApi();

app.Run();

public record Product(long Id, string Name);