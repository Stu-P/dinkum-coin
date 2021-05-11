using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DinkumCoin.Blockchain.Data.Clients
{
    public class ExtendedDynamoDbClient : IDynamoDbClient
    {
        private readonly IAmazonDynamoDB _dbClient;
        private readonly string _tableName;
        private readonly ILogger<ExtendedDynamoDbClient> _logger;

        public ExtendedDynamoDbClient(IAmazonDynamoDB dbClient, ILogger<ExtendedDynamoDbClient> logger, IOptions<DynamoDbClientSettings> settings)
        {
            _dbClient = dbClient ?? throw new ArgumentNullException(nameof(dbClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tableName = settings.Value.TableName;
        }

        public async Task<Document> GetItem(long key)
        {
            try
            {
                var result = await _dbClient.GetItemAsync(
                    new GetItemRequest
                    {
                        TableName = _tableName,
                        Key = new Dictionary<string, AttributeValue>
                           {
                           { "BlockId" , new AttributeValue{ N = key.ToString() } }
                           }
                    });

                if (result.Item.Any())
                {
                    return Document.FromAttributeMap(result.Item);
                }
                _logger.LogInformation("No item found for {key}", key);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occuring attempting to retrieve {key}", key);
                return null;
            }
        }

        public async Task<IEnumerable<Document>> GetAll()
        {
            _logger.LogInformation("Scanning {tableName}", _tableName);
            try
            {
                var result = await _dbClient.ScanAsync(
                    new ScanRequest
                    {
                        TableName = _tableName
                    });

                if (result.Items.Any())
                {
                    _logger.LogInformation("Found {count} items", result.ScannedCount);
                    return result.Items.Select(item => Document.FromAttributeMap(item));
                }
                _logger.LogInformation("Scan returned no items");
                return null;
            }
            catch (ResourceNotFoundException)
            {
                return null;
            }
        }

        public async Task Upsert(Document data)
        {
            try
            {
                var request = new PutItemRequest
                {
                    Item = data.ToAttributeMap(),
                    TableName = _tableName,
                };

                var response = await _dbClient.PutItemAsync(request, CancellationToken.None);
                if (response.HttpStatusCode == HttpStatusCode.BadRequest)
                {
                    _logger.LogError("dynamodb upsert failed with bad request for document {data}", data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during dynamodb upsert for document {data}", data);
            }
        }

        public async Task BatchUpsert(IList<Document> data)
        {
            try
            {
                IEnumerable<WriteRequest> itemsToInsert = data.Select(x =>
                    new WriteRequest(
                        new PutRequest(
                            x.ToAttributeMap()
                    )));

                var request = new BatchWriteItemRequest
                {
                    RequestItems = new Dictionary<string, List<WriteRequest>>
                    {
                        { _tableName, itemsToInsert.ToList() },
                    }
                };

                BatchWriteItemResponse response = await _dbClient.BatchWriteItemAsync(request, CancellationToken.None);
                if (response.HttpStatusCode == HttpStatusCode.BadRequest)
                {
                    throw new Exception($"Batch write failed, requestid: ${response.ResponseMetadata.RequestId}");
                }
                _logger.LogInformation("Successfully wrote batch of {count} documents", data.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during dynamodb batch upsert for {count} documents", data.Count());
            }
        }

        public async Task<IEnumerable<Document>> QueryBy(string attributeName, string attributeValue, string indexName = null)
        {
            var expAttrValues = new Dictionary<string, AttributeValue>
            {
                [":key"] = new AttributeValue { S = attributeValue }
            };

            var request = new QueryRequest
            {
                IndexName = indexName,
                TableName = _tableName,
                KeyConditionExpression = $"{attributeName} = :key",
                ExpressionAttributeValues = expAttrValues
            };

            QueryResponse result = await _dbClient.QueryAsync(request);

            var queryResult = new List<Document>();
            foreach (Dictionary<string, AttributeValue> item in result.Items)
            {
                queryResult.Add(Document.FromAttributeMap(item));
            }
            return queryResult;
        }

        public async Task<bool> AppendItemToList(string keyName, long keyValue, string attributeName, Document item)
        {
            var uir = new UpdateItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue> { { keyName, new AttributeValue { N = keyValue.ToString() } } },
                ExpressionAttributeNames = new Dictionary<string, string>() {
                                { "#AN", attributeName }
                         },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>() {
                    {
                        ":val", new AttributeValue { L= new List<AttributeValue> { new AttributeValue{ M = item.ToAttributeMap() } } }
                    }
                },
                UpdateExpression = $"SET #AN = list_append(#AN, :val)"
            };

            UpdateItemResponse result = await _dbClient.UpdateItemAsync(uir);
            return result.HttpStatusCode == HttpStatusCode.OK ? true : false;
        }

        public async Task<bool> InsertIntoMap(string keyName, long keyValue, string attributeName, string mapKey, Document mapValue)
        {
            _logger.LogInformation("Inserting object into map {mapKey} for {keyName} ", mapKey, keyName);
            var uir = new UpdateItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue> { { keyName, new AttributeValue { N = keyValue.ToString() } } },
                ExpressionAttributeNames = new Dictionary<string, string>() {
                                { "#AN", attributeName },
                                { "#P", mapKey }
                         },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>() {
                    { ":val", new AttributeValue { M = mapValue.ToAttributeMap() } }
                },
                UpdateExpression = $"SET #AN.#P = :val"
            };

            try
            {
                UpdateItemResponse response = await _dbClient.UpdateItemAsync(uir);
                if (response.HttpStatusCode == HttpStatusCode.BadRequest)
                {
                    _logger.LogError("Failed to update document, API returned Bad Request");
                    return false;
                }
                _logger.LogInformation("Successfully inserted into map");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occuring during Update request");
                return false;
            }
        }

        public async Task<bool> ClearSet(string keyName, long keyValue, string attributeName)
        {
            var uir = new UpdateItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue> { { keyName, new AttributeValue { N = keyValue.ToString() } } },
                ExpressionAttributeNames = new Dictionary<string, string>() {
                                { "#AN", attributeName }
                         },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                { ":empty", new AttributeValue { IsLSet = true } },
            },
                UpdateExpression = $"Set #AN = :empty"
            };

            UpdateItemResponse result = await _dbClient.UpdateItemAsync(uir);

            return result.HttpStatusCode == HttpStatusCode.OK ? true : false;
        }
    }
}
