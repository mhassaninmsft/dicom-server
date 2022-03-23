// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Dicom.Core.Features.Model;
using Microsoft.Health.Dicom.CosmosDb.Config;
using EnsureThat;
using Microsoft.Azure.Cosmos;
using System.Text.Json;
using Microsoft.IO;
using Microsoft.Health.Dicom.Core.Features.Query;
using Microsoft.Health.Dicom.Core.Features.Query.Model;
//using Microsoft.Health.Dicom.Core.Models;

namespace Microsoft.Health.Dicom.CosmosDb
{
    public partial class CosmosDataStore : IDisposable, IQueryStore
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

            public string? SerializedValue { get; init; }
        }
    }
}
