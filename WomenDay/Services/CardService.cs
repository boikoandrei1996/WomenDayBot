using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
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
    private readonly CardConfigurationRepository _cardConfigurationRepository;

    public CardService(
      CardConfigurationRepository cardConfigurationRepository)
    {
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

      // AttachmentLayoutTypes.Carousel max item 10
      foreach (var configuration in cardConfigurations.Take(10))
      {
        var cardTemplate = adaptiveCardTemplate
          .Replace("__TitleText__", configuration.TitleText)
          .Replace("__OrderType__", configuration.OrderType)
          .Replace("__OrderCategory__", configuration.OrderCategory)
          .Replace("__Description__", configuration.Description)
          .Replace("__ImageUrl__", configuration.ImageUrl);

        cards.Add(new Attachment()
        {
          ContentType = "application/vnd.microsoft.card.adaptive",
          Content = JsonConvert.DeserializeObject(cardTemplate),
        });
      }

      return cards;
    }
  }
}
