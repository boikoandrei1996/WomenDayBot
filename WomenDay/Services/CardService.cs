﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WomenDay.Models;

namespace WomenDay.Services
{
  public interface ICardService
  {
    Task<List<Attachment>> CreateAttachmentsAsync(OrderCategory category);
  }

  public class CardService : ICardService
  {
    private readonly ILogger<CardService> _logger;
    private readonly ICardConfigurationService _cardConfigurationService;

    public CardService(ILoggerFactory loggerFactory, ICardConfigurationService cardConfigurationService)
    {
      _logger = loggerFactory.CreateLogger<CardService>();
      _cardConfigurationService = cardConfigurationService;
    }

    public Task<List<Attachment>> CreateAttachmentsAsync(OrderCategory category)
    {
      return this.CreateAdaptiveCardAttachmentAsync(category);
    }

    private async Task<List<Attachment>> CreateAdaptiveCardAttachmentAsync(OrderCategory category)
    {
      string[] paths = { ".", "Templates", "orderCard.json" };
      var fullPath = Path.Combine(paths);
      var adaptiveCardTemplate = File.ReadAllText(fullPath);

      var cards = new List<Attachment>();

      var cardConfigurations = await _cardConfigurationService.GetCardConfigurationsAsync();

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
