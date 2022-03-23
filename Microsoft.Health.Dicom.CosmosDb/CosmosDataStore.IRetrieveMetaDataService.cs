// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------



using Microsoft.Health.Dicom.Core.Features.Retrieve;
using Microsoft.Health.Dicom.Core.Messages.Retrieve;

namespace Microsoft.Health.Dicom.CosmosDb
{
    public partial class CosmosDataStore : IRetrieveMetadataService
    {

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

    }
}
