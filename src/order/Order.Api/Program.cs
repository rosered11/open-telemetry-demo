using System;
using System.Collections.Generic;
using System.IO;
using Confluent.Kafka.Extensions.OpenTelemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Order.Api;
using Order.Api.Kafka;
using Serilog;


var builder = WebApplication
    .CreateBuilder(args);
builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true)
        .Build())
    .CreateLogger();

// Add services to the container.
builder.Services.AddOpenTelemetry()
    .ConfigureResource(rb => rb.AddService(builder.Environment.ApplicationName))
    .WithTracing(b =>
    {
        b.AddSource(ServiceName.Orders)
            
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddConfluentKafkaInstrumentation()
            .SetSampler(new ParentBasedSampler(new AlwaysOnSampler()))
            .AddOtlpExporter(x => x.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")!));
    }).WithMetrics(meterBuilder => meterBuilder
        // .AddMeter("Demo")
        // .AddMeter("OpenFeature")
        .AddProcessInstrumentation()
        .AddRuntimeInstrumentation()
        .AddAspNetCoreInstrumentation()
        .SetExemplarFilter(ExemplarFilterType.TraceBased)
        .AddOtlpExporter(x => x.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")!)));

    

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