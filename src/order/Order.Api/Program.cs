using System;
using Confluent.Kafka.Extensions.OpenTelemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Order.Api;
using Order.Api.Kafka;

var builder = WebApplication.CreateBuilder(args);
builder.Logging
    .AddOpenTelemetry(options => options.AddOtlpExporter())
    .AddConsole();
// var tracerProvider = Sdk.CreateTracerProviderBuilder()
//     .AddSource("Order.Producer") // match your ActivitySource name
//     .AddConsoleExporter() // or OTLP exporter to collector
//     .Build();

// Add services to the container.
builder.Services.AddOpenTelemetry()
    .ConfigureResource(rb => rb.AddService(builder.Environment.ApplicationName))
    
  .WithTracing(b =>
  {
      b.AddSource(ServiceName.Orders)
      .AddHttpClientInstrumentation()
      .AddAspNetCoreInstrumentation()
      .AddConfluentKafkaInstrumentation()
      .AddOtlpExporter();
  })
    // .WithMetrics(meterBuilder => meterBuilder
    //     .AddMeter("OpenTelemetry.Demo.Cart")
    //     .AddMeter("OpenFeature")
    //     .AddProcessInstrumentation()
    //     .AddRuntimeInstrumentation()
    //     .AddAspNetCoreInstrumentation()
    //     .SetExemplarFilter(ExemplarFilterType.TraceBased)
    //     .AddOtlpExporter())
    ;

    

builder.Services.AddHttpClient("ProductClient", client =>
{
    client.BaseAddress = new Uri("http://product:80");
});
builder.Services.AddSingleton<Producer>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();