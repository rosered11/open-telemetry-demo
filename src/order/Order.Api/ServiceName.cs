using System.Diagnostics;

namespace Order.Api;

public static class ServiceName
{
    public const string Orders = "Orders.Api";
    public static ActivitySource MyActivitySource = new(Orders);
}