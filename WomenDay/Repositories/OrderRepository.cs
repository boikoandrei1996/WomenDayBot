using Microsoft.Bot.Builder.Azure;
using WomenDay.Models;

namespace WomenDay.Repositories
{
  public sealed class OrderRepository : CosmosDbRepository<Order>
  {
    private const string DatabaseId = "WomenDayBot";
    private const string CollectionId = "Orders";

    public OrderRepository(CosmosDbStorageOptions configurationOptions)
      : base(configurationOptions, OrderRepository.DatabaseId, OrderRepository.CollectionId) { }
  }
}
