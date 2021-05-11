using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;

namespace DinkumCoin.Blockchain.Data.Clients
{
    public interface IDynamoDbClient
    {
        Task<Document> GetItem(long key);
        Task<IEnumerable<Document>> GetAll();
        Task BatchUpsert(IList<Document> data);
        Task Upsert(Document data);
        Task<bool> InsertIntoMap(string keyName, long keyValue, string attributeName, string mapKey, Document mapValue);
        Task<bool> AppendItemToList(string keyName, long keyValue, string attributeName, Document mapValue);
        Task<bool> ClearSet(string keyName, long keyValue, string attributeName);
    }
}
