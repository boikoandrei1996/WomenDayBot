using Microsoft.Bot.Builder.Azure;
using WomenDay.Models;

namespace WomenDay.Repositories
{
  public class CardConfigurationRepository : CosmosDbRepository<CardConfiguration>
  {
    private const string DatabaseId = "WomanDayBot";
    private const string CollectionId = "CardConfiguration";

    public CardConfigurationRepository(CosmosDbStorageOptions configurationOptions)
      : base(configurationOptions, DatabaseId, CollectionId) { }
  }
}
