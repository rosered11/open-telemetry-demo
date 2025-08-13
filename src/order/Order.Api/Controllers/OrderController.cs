using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Order.Api.Kafka;

namespace Order.Api.Controllers;

public record Order(long ProductId);

[ApiController]
[Route("api/[controller]")]
public class OrderController(IHttpClientFactory httpClientFactory,
    Producer producer)
    : ControllerBase
{
    [HttpPost(Name = "Order")]
    public async ValueTask<IActionResult> CreateOrder(Order order)
    {
        var httpClient = httpClientFactory.CreateClient("ProductClient");
        var response = await httpClient.GetAsync("api/products");
        if (!response.IsSuccessStatusCode)
            return BadRequest();
        var products = await response.Content.ReadAsStringAsync();
        var productArray = JsonDocument.Parse(products);
        JsonElement root = productArray.RootElement;
        long id = 0;
        foreach (JsonElement item in root.EnumerateArray())
        {
            if (item.TryGetProperty("id", out JsonElement idElement))
            {
                id = idElement.GetInt64();
            }
        }

        await producer.ProducerAsync();
        
        return Ok(new Order(id));
    }
}