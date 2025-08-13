

using RoseredOtel;

internal static partial class Log
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Order details: {@OrderResult}.")]
    public static partial void OrderReceivedMessage(ILogger logger, OrderResult orderResult);
}