using Microsoft.Bot.Builder.Azure;
using WomenDay.Models;

namespace WomenDay.Repositories
{
  public sealed class CardConfigurationRepository : CosmosDbRepository<CardConfiguration>
  {
    private const string DatabaseId = "WomenDayBot";
    private const string CollectionId = "CardConfigs";

    public CardConfigurationRepository(CosmosDbStorageOptions configurationOptions)
      : base(configurationOptions, CardConfigurationRepository.DatabaseId, CardConfigurationRepository.CollectionId) { }
  }
}
