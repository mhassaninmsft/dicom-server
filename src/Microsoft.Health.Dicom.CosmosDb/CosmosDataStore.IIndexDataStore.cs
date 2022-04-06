// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------



using FellowOakDicom;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;
using Microsoft.Health.Dicom.Core.Features.Model;
using Microsoft.Health.Dicom.Core.Features.Store;

namespace Microsoft.Health.Dicom.CosmosDb
{
    public partial class CosmosDataStore : IIndexDataStore
    {
        public Task<long> BeginCreateInstanceIndexAsync(int partitionKey, DicomDataset dicomDataset, IEnumerable<QueryTag> queryTags, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Method Not Implemented");
            return Task.FromResult(42L);
        }

        public Task DeleteDeletedInstanceAsync(VersionedInstanceIdentifier versionedInstanceIdentifier, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Method Not Implemented");
            return Task.CompletedTask;
        }

        public Task DeleteInstanceIndexAsync(int partitionKey, string studyInstanceUid, string seriesInstanceUid, string sopInstanceUid, DateTimeOffset cleanupAfter, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Method Not Implemented");
            return Task.CompletedTask;
        }

        public Task DeleteSeriesIndexAsync(int partitionKey, string studyInstanceUid, string seriesInstanceUid, DateTimeOffset cleanupAfter, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Method Not Implemented");
            return Task.CompletedTask;
        }

        public Task DeleteStudyIndexAsync(int partitionKey, string studyInstanceUid, DateTimeOffset cleanupAfter, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Method Not Implemented");
            return Task.CompletedTask;
        }
        public Task EndCreateInstanceIndexAsync(int partitionKey, DicomDataset dicomDataset, long watermark, IEnumerable<QueryTag> queryTags, bool allowExpiredTags = false, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Method Not Implemented");
            return Task.CompletedTask;
        }
        public Task<DateTimeOffset> GetOldestDeletedAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Method Not Implemented");
            return Task.FromResult(DateTimeOffset.Now);
        }

        public Task<int> IncrementDeletedInstanceRetryAsync(VersionedInstanceIdentifier versionedInstanceIdentifier, DateTimeOffset cleanupAfter, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Method Not Implemented");
            return Task.FromResult(1);
        }

        public Task ReindexInstanceAsync(DicomDataset dicomDataset, long watermark, IEnumerable<QueryTag> queryTags, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Method Not Implemented");
            return Task.CompletedTask;
        }

        public Task<IEnumerable<VersionedInstanceIdentifier>> RetrieveDeletedInstancesAsync(int batchSize, int maxRetries, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Method Not Implemented");
            return Task.FromResult(new List<VersionedInstanceIdentifier>() as IEnumerable<VersionedInstanceIdentifier>);
        }

        public Task<int> RetrieveNumExhaustedDeletedInstanceAttemptsAsync(int maxNumberOfRetries, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Method Not Implemented");
            return Task.FromResult(1);
        }
    }
}
