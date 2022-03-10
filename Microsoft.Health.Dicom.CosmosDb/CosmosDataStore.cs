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
//using Newtonsoft.Json;
using System.Text.Json;
using Newtonsoft.Json;
using Microsoft.IO;

namespace Microsoft.Health.Dicom.CosmosDb
{
    public class CosmosDataStore : IIndexDataStore, IDisposable, IMetadataStore, IFileStore
    {
        private readonly ILogger<CosmosDataStore> _logger;
        private readonly CosmosDbConfig _config;
        //TODO: add it to DI
        private readonly JsonSerializerOptions _jsonSerilzierSettings;// = new JsonSerializerSettings() { MaxDepth = 2, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private Container Container
        {
            get
            { return _client.GetContainer(_config.DatabaseId, _config.ContainerId); }
        }
        private readonly CosmosClient _client;
        public CosmosDataStore(ILogger<CosmosDataStore> logger, IOptions<CosmosDbConfig> cosmosOptions, IOptions<JsonSerializerOptions> jsonSerializerOptions,
            RecyclableMemoryStreamManager recyclableMemoryStreamManager)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(cosmosOptions, nameof(cosmosOptions));
            EnsureArg.IsNotNull(jsonSerializerOptions, nameof(jsonSerializerOptions));
            EnsureArg.IsNotNull(recyclableMemoryStreamManager, nameof(recyclableMemoryStreamManager));
            _logger = logger;
            _config = cosmosOptions.Value;
            var opts = new CosmosSerializationOptions() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase };
            var cosmosClientOptions = new CosmosClientOptions() { ConnectionMode = ConnectionMode.Direct, SerializerOptions = opts };
            _client = new CosmosClient(_config.EndpointUri.ToString(), _config.PrimaryKey, cosmosClientOptions);
            _jsonSerilzierSettings = jsonSerializerOptions.Value;
            _recyclableMemoryStreamManager = recyclableMemoryStreamManager;

        }
        public Task<long> BeginCreateInstanceIndexAsync(int partitionKey, DicomDataset dicomDataset, IEnumerable<QueryTag> queryTags, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Begin Create Index Async");
            return Task.FromResult(42L);
            //throw new NotImplementedException();
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
            _logger.LogInformation("EndCreateInstanceIndexAsync");
            return Task.CompletedTask;
            //throw new NotImplementedException();
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
            var example = System.Text.Json.JsonSerializer.Serialize(dicomDatasetWithoutBulkData, _jsonSerilzierSettings);

            dynamic? data_json = JsonConvert.DeserializeObject<dynamic>(example);
            var data = new DataField() { Id = Guid.NewGuid().ToString(), Value = data_json ?? "None" };
            _logger.LogInformation("Trying to upload {Object} to Cosmos", data);
            var res = await Container.CreateItemAsync(data, cancellationToken: cancellationToken);
            _logger.LogInformation("Done with uploading to Cosmos");

            //try
            //{
            //    await using (Stream stream = _recyclableMemoryStreamManager.GetStream("dsadsadsa"))
            //    await using (Utf8JsonWriter utf8Writer = new Utf8JsonWriter(stream))
            //    {
            //        // TODO: Use SerializeAsync in .NET 6
            //        System.Text.Json.JsonSerializer.Serialize(utf8Writer, dicomDatasetWithoutBulkData, _jsonSerilzierSettings);
            //        await utf8Writer.FlushAsync(cancellationToken);
            //        stream.Seek(0, SeekOrigin.Begin);
            //        ;
            //    }
            //}
            //catch (Exception)
            //{
            //    throw;
            //}
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
