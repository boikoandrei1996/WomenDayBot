using System.Collections.Generic;
using System.Threading.Tasks;
using WomenDay.Models;
using WomenDay.Repositories;

namespace WomenDay.Services
{
  public interface ICardConfigurationService
  {
    Task<IEnumerable<CardConfiguration>> GetCardConfigurationsAsync();
  }

  public class CardConfigurationService : ICardConfigurationService
  {
    private readonly CardConfigurationRepository _cardConfigurationRepository;

    public CardConfigurationService(CardConfigurationRepository cardConfigurationRepository)
    {
      _cardConfigurationRepository = cardConfigurationRepository;
    }

    public async Task<IEnumerable<CardConfiguration>> GetCardConfigurationsAsync()
    {
      return await _cardConfigurationRepository.GetItemsAsync();
    }
  }
}
