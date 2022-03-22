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
using Microsoft.Health.Dicom.Core.Features.Query;
using Microsoft.Health.Dicom.Core.Features.Query.Model;
using Microsoft.Health.Dicom.Core.Features.Retrieve;
using Microsoft.Health.Dicom.Core.Messages.Retrieve;
using Microsoft.Health.Dicom.Core.Models;

namespace Microsoft.Health.Dicom.CosmosDb
{
    public partial class CosmosDataStore : IIndexDataStore, IDisposable, IMetadataStore, IQueryStore, IRetrieveMetadataService, IInstanceStore
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
            return Task.FromResult(new List<VersionedInstanceIdentifier>() as IEnumerable<VersionedInstanceIdentifier>);
            //throw new NotImplementedException();
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
            var versionedInstanceId = dicomDatasetWithoutBulkData.ToVersionedInstanceIdentifier(version);
            var versionString = versionedInstanceId.SopInstanceUid;
            var data = new DataField()
            {
                Id = versionString, Value = data_json ?? "None", Version = versionedInstanceId.Version,
                StudyId = versionedInstanceId.StudyInstanceUid, SeriesId = versionedInstanceId.SeriesInstanceUid, SopInstanceId = versionedInstanceId.SopInstanceUid
            };
            _logger.LogInformation("Trying to upload {Object} to Cosmos", data);
            var res = await Container.CreateItemAsync(data, cancellationToken: cancellationToken);
            _logger.LogInformation("Done with uploading to Cosmos");
        }


        public async Task<DicomDataset> GetInstanceMetadataAsync(VersionedInstanceIdentifier versionedInstanceIdentifier, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(versionedInstanceIdentifier, nameof(versionedInstanceIdentifier));
            var item = await GetItemById(versionedInstanceIdentifier);
            // Change here
            //versionedInstanceIdentifier.SeriesInstanceUid
            //return await JsonSerializer.DeserializeAsync<DicomDataset>(stream, _jsonSerializerOptions, t);

            //TODO: There should be more performant way of doing this
            //var serizlizedData = JsonConvert.SerializeObject(item.Value);
            //var dicomDataSet = JsonConvert.DeserializeObject<DicomDataset>(serizlizedData);
            var serializedData = System.Text.Json.JsonSerializer.Serialize(item, _jsonSerilzierSettings);
            var dicomDataSet = System.Text.Json.JsonSerializer.Deserialize<DicomDataset>(serializedData, _jsonSerilzierSettings) ?? throw new ArgumentException("deserilzied data is null");
            return dicomDataSet;
        }

        public Task DeleteInstanceMetadataIfExistsAsync(VersionedInstanceIdentifier versionedInstanceIdentifier, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }


        public async Task<QueryResult> QueryAsync(int partitionKey, QueryExpression query, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(query, nameof(query));
            var queryResource = query.QueryResource;
            var filterConditions = query.FilterConditions;
            foreach (var kv in filterConditions)
            {
                _logger.LogInformation("kv is {Kv}", kv);
            }
            var res1 = await GetItems();
            var qresult = new QueryResult(new List<VersionedInstanceIdentifier>());
            //var list1 = res1.Select(s => s.Id).Where(s => s!.Length >= 30).ToList();
            var list2 = res1.Select(s => new VersionedInstanceIdentifier(s.StudyId, s.SeriesId, s.SopInstanceId, s.Version)).ToList() ?? new List<VersionedInstanceIdentifier>();
            return new QueryResult(list2);

            //throw new NotImplementedException();
        }

        public Task<RetrieveMetadataResponse> RetrieveStudyInstanceMetadataAsync(string studyInstanceUid, string ifNoneMatch = null!, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<RetrieveMetadataResponse> RetrieveSeriesInstanceMetadataAsync(string studyInstanceUid, string seriesInstanceUid, string ifNoneMatch = null!, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<RetrieveMetadataResponse> RetrieveSopInstanceMetadataAsync(string studyInstanceUid, string seriesInstanceUid, string sopInstanceUid, string ifNoneMatch = null!, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<VersionedInstanceIdentifier>> GetInstanceIdentifiersInStudyAsync(int partitionKey, string studyInstanceUid, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<VersionedInstanceIdentifier>> GetInstanceIdentifiersInSeriesAsync(int partitionKey, string studyInstanceUid, string seriesInstanceUid, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<VersionedInstanceIdentifier>> GetInstanceIdentifierAsync(int partitionKey, string studyInstanceUid, string seriesInstanceUid, string sopInstanceUid, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
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

        internal class DataField
        {
            public string? Id { get; init; }
            public string? StudyId { get; init; }
            public string? SeriesId { get; init; }
            public string? SopInstanceId { get; init; }
            public long Version { get; init; }

            //[flatten()]
            //TODO: change it to PayLoad
            public dynamic? Value { get; init; }
        }
    }
}
