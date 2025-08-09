using OpenTelemetry.Trace;
using System.Diagnostics;
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
Action<ResourceBuilder> appResourceBuilder =
    resource => resource
        .AddService(builder.Environment.ApplicationName);
        // .AddContainerDetector()
        // .AddHostDetector();
// builder.Services.AddOpenTelemetry()
//     .ConfigureResource(appResourceBuilder)
//   .WithTracing(b =>
//   {
//       b
//       .AddHttpClientInstrumentation()
//       .AddAspNetCoreInstrumentation()
//       .AddOtlpExporter();
//   });
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(rb => rb.AddService(builder.Environment.ApplicationName))
            .WithTracing(tracerProvider =>
            {
                tracerProvider
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri("http://otel-collector:4317"); // match your config
                    });
            });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var activity = Activity.Current;
        activity?.SetTag("app.user.id", "test");
        // activity?.SetTag("app.product.id", request.Item.ProductId);
        // activity?.SetTag("app.product.quantity", request.Item.Quantity);v
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}