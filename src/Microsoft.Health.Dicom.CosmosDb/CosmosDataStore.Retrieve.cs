// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Dicom.Core.Features.Model;
using Microsoft.Health.Dicom.Core.Features.Query;

namespace Microsoft.Health.Dicom.CosmosDb
{
    public partial class CosmosDataStore
    {
        private async Task<IEnumerable<DataField>> GetItems(IReadOnlyCollection<QueryFilterCondition> filterConditions)
        {
            CosmosDbQueryGenerator cosmosDbQueryGenerator = new CosmosDbQueryGenerator();
            var maxItemCount = 100;
            var query = "";
            foreach (var condition in filterConditions)
            {
                condition.Accept(cosmosDbQueryGenerator);
            }

            // FUTURE TODO: continuation tokens ? will need a POST for a long token
            query = $"SELECT * FROM c WHERE ({cosmosDbQueryGenerator.OutputQuery()})";
            _logger.LogInformation("the query is : {Query}", query);
            // FUTURE TODO: get from URL parameter
            string continuation = "";
            var queryOptions = new QueryRequestOptions()
            {
                MaxItemCount = maxItemCount,
            };
            // TODO: if continuation token not null/empty, then add it to teh query iterator
            var res1 = Container.GetItemQueryIterator<DataField>(query, requestOptions: queryOptions);
            var list = new List<DataField>();

            using (res1)
            {
                while (res1.HasMoreResults)
                {
                    FeedResponse<DataField> response = await res1.ReadNextAsync();
                    if (response.Count > 0)
                    {
                        list.AddRange(response.Resource);
                        continuation = response.ContinuationToken;
                        break;
                    }
                }
            }
            if (!String.IsNullOrEmpty(continuation))
            {
                //FUTURE TODO: return token with list for pagination?
                _logger.LogInformation("Continuation Token:  {Continuation_Token}", continuation);
            }

            return list;
        }

        private async Task<DataField> GetItemById(VersionedInstanceIdentifier versionedInstanceIdentifier)
        {
            //var data = new DataField() { Value = new { val = 45 } };
            var id = GetIdFromVersionedInstanceIdentifier(versionedInstanceIdentifier);
            var query = $"SELECT * FROM c WHERE c.id='{id}'";
            var res1 = Container.GetItemQueryIterator<DataField>(query);
            var list = new List<DataField>();
            // If there is more than one instance , we should throw
            // So there should be exactly one instance
            while (res1.HasMoreResults)
            {
                foreach (var item in await res1.ReadNextAsync())
                {
                    list.Add(item);
                }
            }
            if (list.Count != 1)
            {
                //TODO: What kind of exception to throw (use constraint violation exception)
                throw new ArgumentOutOfRangeException($"Expected List to contain exactly one item with id {id}");
            }

            return list[0];

        }

        private async Task DeleteItembyId(VersionedInstanceIdentifier versionedInstanceIdentifier)
        {
            var id = GetIdFromVersionedInstanceIdentifier(versionedInstanceIdentifier);
            var instanceToBeDeleted = new DataField() { Id = id };
            var result = await Container.DeleteItemAsync<DataField>(id, new Azure.Cosmos.PartitionKey(id));

        }
    }
}
