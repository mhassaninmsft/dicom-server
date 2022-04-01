// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using FellowOakDicom;

using Microsoft.Health.Dicom.Core.Features.Model;
using Microsoft.Health.Dicom.Core.Features.Query;

namespace Microsoft.Health.Dicom.CosmosDb
{
    public partial class CosmosDataStore
    {
        private async Task<IEnumerable<DataField>> GetItems(IReadOnlyCollection<QueryFilterCondition> filterConditions)
        {
            CosmosDbQueryGenerator cosmosDbQueryGenerator = new CosmosDbQueryGenerator();
            var query = "";
            foreach (var condition in filterConditions)
            {
                Console.WriteLine($" THE CONDITIONS ARE THIS: {condition.ToString}");
                condition.Accept(cosmosDbQueryGenerator);
            }
            // Take the conditions and populate into the SQL query
            // **** TODO **** This should ultimately be written differently to potentially avoid sql injection vulnerabilities
            //INSPO? QueryDefinition query = new QueryDefinition("SELECT * FROM Families f WHERE f.id = @id AND f.Address.City = @city")
            //    .WithParameter("@id", "AndersonFamily")
            //    .WithParameter("@city", "Seattle");

            query = $"SELECT * FROM c WHERE {cosmosDbQueryGenerator.OutputQuery()}";
            Console.WriteLine($"the query is : {query}");
            var res1 = Container.GetItemQueryIterator<DataField>(query);
            var list = new List<DataField>();
            while (res1.HasMoreResults)
            {
                foreach (var item in await res1.ReadNextAsync())
                {
                    list.Add(item);
                }
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
