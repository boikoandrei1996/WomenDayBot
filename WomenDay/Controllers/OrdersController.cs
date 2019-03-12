using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WomenDay.Models;
using WomenDay.Repositories;

namespace WomenDay.Web.Controllers
{
  [Route("api/[controller]")]
  public class OrdersController : Controller
  {
    private readonly ILogger<Bot> _logger;
    private readonly OrderRepository _orderRepository;

    public OrdersController(
      ILogger<Bot> logger, 
      OrderRepository orderRepository)
    {
      _logger = logger;
      _orderRepository = orderRepository;
    }

    // GET api/orders/all
    [HttpGet("all")]
    public async Task<IEnumerable<Order>> GetOrdersAsync()
    {
      var orders = await _orderRepository.GetItemsAsync();

      return orders;
    }

    // PUT api/orders
    [HttpPut]
    public async Task<IActionResult> UpdateOrderAsync([FromBody]Order order)
    {
      if (order == null)
      {
        return BadRequest();
      }

      var doc = await _orderRepository.UpdatePropertyAsync(order.DocumentId.ToString(), nameof(order.IsComplete), order.IsComplete);
      if (doc == null)
      {
        return BadRequest("Document not found.");
      }

      return Ok();
    }
  }
}
