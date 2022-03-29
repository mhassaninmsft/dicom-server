﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Dicom.Core.Features.Export;
public record ExportOperationInput
{
    public ExportOperationSource Source { get; set; }

    public ExportDestination Destination { get; set; }
}
