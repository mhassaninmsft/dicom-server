// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------



using EnsureThat;
using FellowOakDicom;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Dicom.Core.Extensions;
using Microsoft.Health.Dicom.Core.Features.Common;
using Microsoft.Health.Dicom.Core.Features.Model;
using Newtonsoft.Json;

namespace Microsoft.Health.Dicom.CosmosDb
{
    public partial class CosmosDataStore : IMetadataStore
    {
        // IMetaData Member Functions
        public async Task StoreInstanceMetadataAsync(DicomDataset dicomDataset, long version, CancellationToken cancellationToken = default)
        {
            // Creates a copy of the dataset with bulk data removed.
            DicomDataset dicomDatasetWithoutBulkData = dicomDataset.CopyWithoutBulkDataItems();
            var serializedValue = System.Text.Json.JsonSerializer.Serialize(dicomDatasetWithoutBulkData, _jsonSerilzierSettings);

            dynamic? data_json = JsonConvert.DeserializeObject<dynamic>(serializedValue);
            //dynamic data_json = example;
            var versionedInstanceId = dicomDatasetWithoutBulkData.ToVersionedInstanceIdentifier(version);
            var versionString = versionedInstanceId.SopInstanceUid;
            var data = new DataField()
            {
                Id = versionString, Value = data_json ?? "None", Version = versionedInstanceId.Version, SerializedValue = serializedValue,
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
            //string serializedData = System.Text.Json.JsonSerializer.Serialize(item.Value, _jsonSerilzierSettings) ?? throw new ArgumentException("serizlied data can't be null");
            string serializedData = item.SerializedValue ?? throw new ArgumentException("serialized Data should not be null");
            var streamBytes = System.Text.Encoding.UTF8.GetBytes(serializedData);
            using MemoryStream stream = new MemoryStream(streamBytes);
            DicomDataset dc1 = new DicomDataset();
            //dc1.Add( new ;
            var dicomDataSet = await System.Text.Json.JsonSerializer.DeserializeAsync<DicomDataset>(stream, _jsonSerilzierSettings, cancellationToken) ?? throw new ArgumentException("deserilzied data is null");
            return dicomDataSet;
        }

        public Task DeleteInstanceMetadataIfExistsAsync(VersionedInstanceIdentifier versionedInstanceIdentifier, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

    }
}
