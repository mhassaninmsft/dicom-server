// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Core;
using Microsoft.Health.Dicom.Core.Features.Common;
using Microsoft.Health.Dicom.Core.Features.Model;
using Microsoft.Health.Dicom.Core.Features.Retrieve;

namespace Microsoft.Health.Dicom.Core.Features.Delete
{
    public class CosmosDeleteService : IDeleteService
    {
        private readonly IMetadataStore _metadataStore;
        private readonly IFileStore _fileStore;
        private readonly IInstanceStore _instanceStore;
        private readonly ILogger<CosmosDeleteService> _logger;
        public CosmosDeleteService(IMetadataStore metadataStore, IFileStore fileStore, IInstanceStore instanceStore, ILogger<CosmosDeleteService> logger)
        {
            _fileStore = fileStore;
            _metadataStore = metadataStore;
            _instanceStore = instanceStore;
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
            // Partition Key?
            ICollection<VersionedInstanceIdentifier> instancesToRetrieve = Enumerable.Empty<VersionedInstanceIdentifier>().ToList();

            if (!string.IsNullOrEmpty(studyInstanceUid) && !string.IsNullOrEmpty(seriesInstanceUid) && !string.IsNullOrEmpty(sopInstanceUid))
            {
                var versionedInstanceIdentifier = new VersionedInstanceIdentifier(studyInstanceUid, seriesInstanceUid, sopInstanceUid, 42L);
                instancesToRetrieve.Add(versionedInstanceIdentifier);
            }

            else if (string.IsNullOrEmpty(seriesInstanceUid) && string.IsNullOrEmpty(sopInstanceUid))
            {
                var collection = await _instanceStore.GetInstanceIdentifiersInStudyAsync(
                        0,
                        studyInstanceUid,
                        cancellationToken);
                instancesToRetrieve = collection.ToList();
            }
            else if (string.IsNullOrEmpty(sopInstanceUid))
            {
                var collection = await _instanceStore.GetInstanceIdentifiersInSeriesAsync(
                        0,
                        studyInstanceUid,
                        seriesInstanceUid,
                        cancellationToken);
                instancesToRetrieve = collection.ToList();
            }
            foreach (var instanceIdentifier in instancesToRetrieve)
            {
                try
                {
                    await _metadataStore.DeleteInstanceMetadataIfExistsAsync(instanceIdentifier, cancellationToken);
                    // How to delete the file with multiple versions
                    await _fileStore.DeleteFileIfExistsAsync(instanceIdentifier, cancellationToken);
                }
                catch (Exception ex)
                {
                    // We do not stop deleting the instances ,but log the error
                    _logger.LogError("Caught Exception {Excpetion}", ex.ToString());
                }
            }

        }

        public async Task DeleteInstanceNowAsync(string studyInstanceUid, string seriesInstanceUid, string sopInstanceUid, CancellationToken cancellationToken)
        {
            await DeleteInstanceAsync(studyInstanceUid, seriesInstanceUid, sopInstanceUid, cancellationToken);
        }

        public async Task DeleteSeriesAsync(string studyInstanceUid, string seriesInstanceUid, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(studyInstanceUid, nameof(studyInstanceUid));
            EnsureArg.IsNotNullOrEmpty(seriesInstanceUid, nameof(seriesInstanceUid));

            await DeleteInstanceAsync(studyInstanceUid, seriesInstanceUid, sopInstanceUid: null, cancellationToken);
        }

        public async Task DeleteStudyAsync(string studyInstanceUid, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(studyInstanceUid, nameof(studyInstanceUid));

            await DeleteInstanceAsync(studyInstanceUid, seriesInstanceUid: null, sopInstanceUid: null, cancellationToken);
        }

        private static DateTimeOffset GenerateCleanupAfter(TimeSpan delay)
        {
            return Clock.UtcNow + delay;
        }
    }
}
