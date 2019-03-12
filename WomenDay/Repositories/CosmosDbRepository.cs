using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Bot.Builder.Azure;

namespace WomenDay.Repositories
{
  public abstract class CosmosDbRepository<T>
    where T : class
  {
    private readonly string _databaseId;
    private readonly string _collectionId;
    private readonly CosmosDbStorageOptions _dbStorageOptions;
    private readonly Uri _collectionUri;

    public CosmosDbRepository(
      CosmosDbStorageOptions configurationOptions,
      string databaseId,
      string collectionId)
    {
      _databaseId = databaseId;
      _collectionId = collectionId;
      _dbStorageOptions = configurationOptions;
      _collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
    }

    public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate = null)
    {
      using (var client = new DocumentClient(_dbStorageOptions.CosmosDBEndpoint, _dbStorageOptions.AuthKey))
      {
        IQueryable<T> query = client
          .CreateDocumentQuery<T>(_collectionUri, new FeedOptions { EnableCrossPartitionQuery = true });

        if (predicate != null)
        {
          query = query.Where(predicate);
        }

        IDocumentQuery<T> documentQuery = query.AsDocumentQuery();

        var results = new List<T>();
        while (documentQuery.HasMoreResults)
        {
          results.AddRange(await documentQuery.ExecuteNextAsync<T>());
        }

        return results;
      }
    }

    public Document GetDocumentOrDefault(Expression<Func<Document, bool>> predicate)
    {
      if (predicate == null)
      {
        throw new ArgumentNullException(nameof(predicate));
      }

      using (var client = new DocumentClient(_dbStorageOptions.CosmosDBEndpoint, _dbStorageOptions.AuthKey))
      {
        var doc = client
          .CreateDocumentQuery<Document>(_collectionUri, new FeedOptions { EnableCrossPartitionQuery = true })
          .Where(predicate)
          .AsEnumerable()
          .FirstOrDefault();

        return doc;
      }
    }

    public async Task<Document> CreateDocumentAsync(T item)
    {
      using (var client = new DocumentClient(_dbStorageOptions.CosmosDBEndpoint, _dbStorageOptions.AuthKey))
      {
        return await client.CreateDocumentAsync(_collectionUri, item);
      }
    }

    public async Task<Document> UpdateDocumentAsync(Document document)
    {
      using (var client = new DocumentClient(_dbStorageOptions.CosmosDBEndpoint, _dbStorageOptions.AuthKey))
      {
        return await client.ReplaceDocumentAsync(document);
      }
    }

    public async Task<Document> UpdateDocumentAsync(string documentId, T item)
    {
      var documentUri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, documentId);

      using (var client = new DocumentClient(_dbStorageOptions.CosmosDBEndpoint, _dbStorageOptions.AuthKey))
      {
        return await client.ReplaceDocumentAsync(documentUri, item);
      }
    }

    public async Task<Document> UpdatePropertyAsync(string documentId, string propertyName, object propertyValue)
    {
      var document = this.GetDocumentOrDefault(x => x.Id == documentId);
      if (document != null)
      {
        document.SetPropertyValue(propertyName, propertyValue);

        return await this.UpdateDocumentAsync(document);
      }

      return null;
    }

    public async Task<ResourceResponse<Document>> DeleteDocumentAsync(string documentId)
    {
      var documentUri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, documentId);

      using (var client = new DocumentClient(_dbStorageOptions.CosmosDBEndpoint, _dbStorageOptions.AuthKey))
      {
        return await client.DeleteDocumentAsync(documentUri);
      }
    }
  }
}
