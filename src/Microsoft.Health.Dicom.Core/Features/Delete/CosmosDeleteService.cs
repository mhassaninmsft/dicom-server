// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------


using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Dicom.Core.Features.Common;
using Microsoft.Health.Dicom.Core.Features.Model;

namespace Microsoft.Health.Dicom.Core.Features.Delete
{
    public class CosmosDeleteService : IDeleteService
    {
        private readonly IMetadataStore _metadataStore;
        private readonly IFileStore _fileStore;
        private readonly ILogger<CosmosDeleteService> _logger;
        public CosmosDeleteService(IMetadataStore metadataStore, IFileStore fileStore, ILogger<CosmosDeleteService> logger)
        {
            _fileStore = fileStore;
            _metadataStore = metadataStore;
            _logger = logger;
        }
        public Task<(bool success, int retrievedInstanceCount)> CleanupDeletedInstancesAsync(CancellationToken cancellationToken = default)
        {
            // No Op since we have no cleanup, we will delete instances immediately
            return Task.FromResult((true, 0));
        }

        public async Task DeleteInstanceAsync(string studyInstanceUid, string seriesInstanceUid, string sopInstanceUid, CancellationToken cancellationToken = default)
        {
            //TODO: How to get the version
            VersionedInstanceIdentifier versionedInstanceIdentifier = new VersionedInstanceIdentifier(studyInstanceUid, seriesInstanceUid, sopInstanceUid, 1L);
            await _metadataStore.DeleteInstanceMetadataIfExistsAsync(versionedInstanceIdentifier, cancellationToken);
            // How to delete the file with multiple versions
            //await _fileStore.DeleteFileIfExistsAsync(versionedInstanceIdentifier, cancellationToken);

        }

        public async Task DeleteInstanceNowAsync(string studyInstanceUid, string seriesInstanceUid, string sopInstanceUid, CancellationToken cancellationToken)
        {
            await DeleteInstanceAsync(studyInstanceUid, seriesInstanceUid, sopInstanceUid, cancellationToken);
        }

        public Task DeleteSeriesAsync(string studyInstanceUid, string seriesInstanceUid, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteStudyAsync(string studyInstanceUid, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
