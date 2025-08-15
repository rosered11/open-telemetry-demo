

using Confluent.Kafka;
using RoseredOtel;

internal static partial class Log
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Order details: {@OrderResult}.")]
    public static partial void OrderReceivedMessage(ILogger logger, OrderResult orderResult);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Consumer Order headers: {Key}.")]
    public static partial void OrderReceivedHeaderKey(ILogger logger, string key);
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Consumer Order headers: {Val}.")]
    public static partial void OrderReceivedHeaderValue(ILogger logger, string val);
}

public record KafkaHeader(string Key, string Value);