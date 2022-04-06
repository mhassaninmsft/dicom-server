// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------



using Microsoft.Health.Dicom.Core.Features.Model;
using Microsoft.Health.Dicom.Core.Features.Retrieve;
using Microsoft.Health.Dicom.Core.Models;

namespace Microsoft.Health.Dicom.CosmosDb
{
    public partial class CosmosDataStore : IInstanceStore
    {

        public Task<IEnumerable<VersionedInstanceIdentifier>> GetInstanceIdentifiersInStudyAsync(int partitionKey, string studyInstanceUid, CancellationToken cancellationToken = default)
        {
            return GetInstanceIdentifierAsync(partitionKey, studyInstanceUid, seriesInstanceUid: null, sopInstanceUid: null, cancellationToken);
        }

        public Task<IEnumerable<VersionedInstanceIdentifier>> GetInstanceIdentifiersInSeriesAsync(int partitionKey, string studyInstanceUid, string seriesInstanceUid, CancellationToken cancellationToken = default)
        {
            return GetInstanceIdentifierAsync(partitionKey, studyInstanceUid, seriesInstanceUid, sopInstanceUid: null, cancellationToken);
        }

        public async Task<IEnumerable<VersionedInstanceIdentifier>> GetInstanceIdentifierAsync(int partitionKey, string studyInstanceUid, string? seriesInstanceUid, string? sopInstanceUid, CancellationToken cancellationToken = default)
        {
            var query = "";

            if (!string.IsNullOrEmpty(studyInstanceUid) && !string.IsNullOrEmpty(seriesInstanceUid) && !string.IsNullOrEmpty(sopInstanceUid))
            {
                query = $"SELECT * FROM c WHERE c.studyId = '{studyInstanceUid}' AND c.seriesId = '{seriesInstanceUid}' AND c.sopInstanceId = '{sopInstanceUid}' ";
            }

            else if (string.IsNullOrEmpty(seriesInstanceUid) && string.IsNullOrEmpty(sopInstanceUid))
            {
                query = $"SELECT * FROM c WHERE c.studyId = '{studyInstanceUid}'";
            }
            else if (string.IsNullOrEmpty(sopInstanceUid))
            {
                query = $"SELECT * FROM c WHERE c.studyId = '{studyInstanceUid}' AND c.seriesId = '{seriesInstanceUid}'";
            }

            var res1 = Container.GetItemQueryIterator<DataField>(query);
            var list = new List<VersionedInstanceIdentifier>();

            while (res1.HasMoreResults)
            {
                foreach (var item in await res1.ReadNextAsync(cancellationToken))
                {
                    list.Add(new VersionedInstanceIdentifier(item.StudyId, item.SeriesId, item.SopInstanceId, item.Version, partitionKey));
                }
            }
            return list;
        }

        public Task<IReadOnlyList<VersionedInstanceIdentifier>> GetInstanceIdentifiersByWatermarkRangeAsync(WatermarkRange watermarkRange, IndexStatus indexStatus, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<WatermarkRange>> GetInstanceBatchesAsync(int batchSize, int batchCount, IndexStatus indexStatus, long? maxWatermark = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<InstanceMetadata>> GetInstanceIdentifierWithPropertiesAsync(int partitionKey, string studyInstanceUid, string seriesInstanceUid = null!, string sopInstanceUid = null!, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
