using System.Threading.Tasks;
using Microsoft.Azure.Documents;
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

    public async Task<Document> UpdatePropertyAsync(string documentId, string propertyName, object propertyValue)
    {
      var doc = base.GetDocumentOrDefault(x => x.Id == documentId);
      if (doc == null)
      {
        return null;
      }

      doc.SetPropertyValue(propertyName, propertyValue);

      var updatedDoc = await base.UpdateDocumentAsync(doc);

      return updatedDoc;
    }
  }
}
