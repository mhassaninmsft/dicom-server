// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Health.Dicom.Core.Features.Export.Model;

namespace Microsoft.Health.Dicom.Core.Features.Export;
public record ExportOperationSource
{
    public IReadOnlySet<Study> Studies { get; set; }
    // Encompasses Multiple Series, the word Series is both the Singular and Plural Form
    public IReadOnlySet<Series> Series { get; set; }
    public IReadOnlySet<Instance> Instances { get; set; }

}
