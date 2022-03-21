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

namespace Microsoft.Health.Dicom.CosmosDb
{
    public partial class CosmosDataStore
    {
        private async Task<IEnumerable<DataField>> GetItems()
        {
            //var data = new DataField() { Value = new { val = 45 } };
            var query = "SELECT * FROM c WHERE c['value']['00080020']['Value'][0] = '20200922'";
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
            //throw new NotImplementedException();
            //return Task.FromResult(new DataField());
            //Container.GetItemLinqQueryable()
        }
    }
}
