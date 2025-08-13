
using System.Diagnostics;

public static class ServiceName
{
    public const string Products = "Products.Api";
    public static ActivitySource MyActivitySource = new(Products);
}