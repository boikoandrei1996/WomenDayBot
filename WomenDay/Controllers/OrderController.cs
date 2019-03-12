using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WomenDay.Models;
using WomenDay.Repositories;

namespace WomenDay.Web.Controllers
{
  [Route("api/[controller]")]
  public class OrderController : Controller
  {
    private readonly OrderRepository _orderRepository;

    public OrderController(
      OrderRepository orderRepository)
    {
      _orderRepository = orderRepository;
    }

    // GET api/order/all
    [HttpGet("all")]
    public async Task<IEnumerable<Order>> GetOrdersAsync()
    {
      var orders = await _orderRepository.GetItemsAsync();

      return orders.OrderBy(x => x.RequestTime);
    }

    // PUT api/order
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
