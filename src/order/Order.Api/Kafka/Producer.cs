using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Extensions.Diagnostics;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RoseredOtel;

namespace Order.Api.Kafka;

public sealed class Producer
{
    private const string TopicName = "orders";
    private readonly ILogger<Producer> _logger;
    private readonly IProducer<string, byte[]> _producer;

    public Producer(ILogger<Producer> logger)
    {
        _logger = logger;

        var servers = "kafka:9092";

        _producer = BuildProducer(servers);

        _logger.LogInformation($"Connecting to Kafka: {servers}");
    }
    public async Task ProducerAsync()
    {
        var propagator = Propagators.DefaultTextMapPropagator;

        using var activity = Activity.Current;//ServiceName.MyActivitySource.StartActivity("order publish", ActivityKind.Producer);
        activity.SetTag("messaging.system", "kafka");
        activity.SetTag("messaging.destination", TopicName);
        activity.SetTag("messaging.operation", "publish");
        // activity.SetTag("messaging.kafka.partition", msg.Partition);
        activity.SetTag("net.transport", "IP.TCP");
        activity.SetTag("peer.service", "kafka");

        var headers = new Headers();
        if (Activity.Current != null)
            propagator.Inject(new PropagationContext(activity.Context, Baggage.Current), headers,
                (h, k, v) => h.Add(k, System.Text.Encoding.UTF8.GetBytes(v)));
        OrderResult orderResult = new(){ OrderId = Guid.NewGuid().ToString() };
        await _producer.ProduceAsync(TopicName, new Message<string, byte[]>
        {
            // Key = "key1",
            Value = orderResult.ToByteArray(),
            Headers = headers
        });
    }
    private IProducer<string, byte[]> BuildProducer(string servers)
    {
        var conf = new ProducerConfig
        {
            BootstrapServers = servers,
        };

        return new ProducerBuilder<string, byte[]>(conf)
            // .SetKeySerializer(Serializers.Null)
            // .SetValueSerializer(Serializers.Utf8)
            .BuildWithInstrumentation();
            // .Build();
    }
}