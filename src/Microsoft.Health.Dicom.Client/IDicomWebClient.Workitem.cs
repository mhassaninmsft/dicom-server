﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dicom;

namespace Microsoft.Health.Dicom.Client
{
    public partial interface IDicomWebClient
    {
        Task<DicomWebResponse> AddWorkitemAsync(IEnumerable<DicomDataset> dicomDatasets, string workitemUid = default, string partitionName = default, CancellationToken cancellationToken = default);
    }
}
