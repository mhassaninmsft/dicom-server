// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using FellowOakDicom;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;
using Microsoft.Health.Dicom.Core.Features.Model;
using Microsoft.Health.Dicom.Core.Features.Store;
using Microsoft.Health.Dicom.CosmosDb.Config;
using EnsureThat;
using Microsoft.Azure.Cosmos;
using Microsoft.Health.Dicom.Core.Features.Common;
using Microsoft.Health.Dicom.Core.Extensions;
using Newtonsoft.Json;

namespace Microsoft.Health.Dicom.CosmosDb
{
    public class CosmosDataStore : IIndexDataStore, IDisposable, IMetadataStore, IFileStore
    {
        private readonly ILogger<CosmosDataStore> _logger;
        private readonly CosmosDbConfig _config;
        private Container Container
        {
            get
            { return _client.GetContainer(_config.DatabaseId, _config.ContainerId); }
        }
        private readonly CosmosClient _client;
        public CosmosDataStore(ILogger<CosmosDataStore> logger, IOptions<CosmosDbConfig> cosmosOptions)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(cosmosOptions, nameof(cosmosOptions));
            _logger = logger;
            _config = cosmosOptions.Value;
            var opts = new CosmosSerializationOptions() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase };
            var cosmosClientOptions = new CosmosClientOptions() { ConnectionMode = ConnectionMode.Direct, SerializerOptions = opts };
            _client = new CosmosClient(_config.EndpointUri.ToString(), _config.PrimaryKey, cosmosClientOptions);
        }
        public Task<long> BeginCreateInstanceIndexAsync(int partitionKey, DicomDataset dicomDataset, IEnumerable<QueryTag> queryTags, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDeletedInstanceAsync(VersionedInstanceIdentifier versionedInstanceIdentifier, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteInstanceIndexAsync(int partitionKey, string studyInstanceUid, string seriesInstanceUid, string sopInstanceUid, DateTimeOffset cleanupAfter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteSeriesIndexAsync(int partitionKey, string studyInstanceUid, string seriesInstanceUid, DateTimeOffset cleanupAfter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteStudyIndexAsync(int partitionKey, string studyInstanceUid, DateTimeOffset cleanupAfter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Put disposing code here
                _client.Dispose();
            }
        }
        public Task EndCreateInstanceIndexAsync(int partitionKey, DicomDataset dicomDataset, long watermark, IEnumerable<QueryTag> queryTags, bool allowExpiredTags = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<DateTimeOffset> GetOldestDeletedAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> IncrementDeletedInstanceRetryAsync(VersionedInstanceIdentifier versionedInstanceIdentifier, DateTimeOffset cleanupAfter, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task ReindexInstanceAsync(DicomDataset dicomDataset, long watermark, IEnumerable<QueryTag> queryTags, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<VersionedInstanceIdentifier>> RetrieveDeletedInstancesAsync(int batchSize, int maxRetries, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> RetrieveNumExhaustedDeletedInstanceAttemptsAsync(int maxNumberOfRetries, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        // IMetaData Member Functions
        public async Task StoreInstanceMetadataAsync(DicomDataset dicomDataset, long version, CancellationToken cancellationToken = default)
        {
            // Creates a copy of the dataset with bulk data removed.
            DicomDataset dicomDatasetWithoutBulkData = dicomDataset.CopyWithoutBulkDataItems();
            var example = JsonConvert.SerializeObject(dicomDatasetWithoutBulkData);
            var data = new DataField() { Id = Guid.NewGuid().ToString(), Value = example };
            _logger.LogInformation("Trying to upload {Object} to Cosmos", data);
            var res = await Container.CreateItemAsync(data, cancellationToken: cancellationToken);
            _logger.LogInformation("Done with uploading to Cosmos");
        }

        public Task<DicomDataset> GetInstanceMetadataAsync(VersionedInstanceIdentifier versionedInstanceIdentifier, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteInstanceMetadataIfExistsAsync(VersionedInstanceIdentifier versionedInstanceIdentifier, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Uri> StoreFileAsync(VersionedInstanceIdentifier versionedInstanceIdentifier, Stream stream, CancellationToken cancellationToken = default)
        {
            var uri1 = new Uri("https://www.google.com");
            return Task.FromResult(uri1);
        }

        public Task<Stream> GetFileAsync(VersionedInstanceIdentifier versionedInstanceIdentifier, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFileIfExistsAsync(VersionedInstanceIdentifier versionedInstanceIdentifier, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        internal class DataField
        {
            public string? Id { get; init; }
            public dynamic? Value { get; init; }
        }
    }
}
