using System.Diagnostics;
using System.Text;
using Confluent.Kafka;
using Confluent.Kafka.Extensions.Diagnostics;
using OpenTelemetry.Context.Propagation;
using RoseredOtel;

public sealed class Consumer : IDisposable
{
    private const string TopicName = "orders";
    private readonly ILogger<Consumer> _logger;
    private readonly IConsumer<string, byte[]> _consumer;
    private bool _isListening;
    private static readonly ActivitySource MyActivitySource = new(ServiceName.Products);
    public Consumer(ILogger<Consumer> logger)
    {
        _logger = logger;

        var servers = "kafka:9092";

        _consumer = BuildConsumer(servers);
        _consumer.Subscribe(TopicName);

        _logger.LogInformation($"Connecting to Kafka: {servers}");
    }
    private IConsumer<string, byte[]> BuildConsumer(string servers)
    {
        var conf = new ConsumerConfig
        {
            GroupId = $"product",
            BootstrapServers = servers,
            // https://github.com/confluentinc/confluent-kafka-dotnet/tree/07de95ed647af80a0db39ce6a8891a630423b952#basic-consumer-example
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        return new ConsumerBuilder<string, byte[]>(conf)
            .SetKeyDeserializer(Deserializers.Utf8)
            .Build();
    }
    public void StartListening()
    {
        _isListening = true;

        try
        {
            while (_isListening)
            {
                try
                {
                    // _consumer.ConsumeWithInstrumentation(record =>
                    // {
                    //     ProcessMessage(record.Message);
                    // }, 1000);
                    var consumeResult = _consumer.Consume();
                    ProcessMessage(consumeResult.Message);
                }
                catch (ConsumeException e)
                {
                    _logger.LogError(e, "Consume error: {0}", e.Error.Reason);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Closing consumer");

            _consumer.Close();
        }
    }

    private void ProcessMessage(Message<string, byte[]> message)
    {
        try
        {
            var parentContext = Propagators.DefaultTextMapPropagator.Extract(
                default,
                message.Headers,
                (headers, key) =>
                {
                    var header = headers.FirstOrDefault(h => h.Key == key);
                    return header == null ? Enumerable.Empty<string>() : new[] { Encoding.UTF8.GetString(header.GetValueBytes()) };
                }
            );
            var activity = MyActivitySource.StartActivity("process order", ActivityKind.Consumer, parentContext.ActivityContext);
            var order = OrderResult.Parser.ParseFrom(message.Value);
            Log.OrderReceivedMessage(_logger, order);
            activity.AddEvent(new("Received message"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Order parsing failed:");
        }
    }
    public void Dispose()
    {
        _isListening = false;
        _consumer?.Dispose();
    }
}