using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WomenDay.Models;
using WomenDay.Repositories;

namespace WomenDay.Services
{
  public interface ICardService
  {
    Task<List<Attachment>> CreateAttachmentsAsync(OrderCategory category);
  }

  public class CardService : ICardService
  {
    private readonly ILogger<CardService> _logger;
    private readonly CardConfigurationRepository _cardConfigurationRepository;

    public CardService(
      ILogger<CardService> logger, 
      CardConfigurationRepository cardConfigurationRepository)
    {
      _logger = logger;
      _cardConfigurationRepository = cardConfigurationRepository;
    }

    public async Task<List<Attachment>> CreateAttachmentsAsync(OrderCategory category)
    {
      var fullPath = Path.Combine(".", "Templates", "CardTemplate.json");
      var adaptiveCardTemplate = File.ReadAllText(fullPath);

      var cards = new List<Attachment>();

      var cardConfigurations = await _cardConfigurationRepository.GetItemsAsync();

      if (category != OrderCategory.All)
      {
        var categoryName = category.ToString();
        cardConfigurations = cardConfigurations.Where(x => categoryName.Equals(x.OrderCategory, StringComparison.OrdinalIgnoreCase));
      }

      foreach (var configuration in cardConfigurations.Take(10))
      {
        var card = adaptiveCardTemplate;
        card = card.Replace(@"__TitleText__", configuration.TitleText);
        card = card.Replace(@"__OrderType__", configuration.OrderType);
        card = card.Replace(@"__OrderCategory__", configuration.OrderCategory);
        card = card.Replace(@"__Description__", configuration.Description);
        card = card.Replace(@"__ImageUrl__", configuration.ImageUrl);

        var adaptiveCard = new Attachment()
        {
          ContentType = "application/vnd.microsoft.card.adaptive",
          Content = JsonConvert.DeserializeObject(card),
        };
        cards.Add(adaptiveCard);
      }

      return cards;
    }
  }
}
