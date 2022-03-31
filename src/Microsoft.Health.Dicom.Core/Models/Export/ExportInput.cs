// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Health.Dicom.Core.Models.Operations;

namespace Microsoft.Health.Dicom.Core.Models.Export;

public class ExportInput
{
    public SourceManifest Manifest { get; set; }

    public ExportDestination Destination { get; set; }

    public BatchOptions Batching { get; set; }
}
