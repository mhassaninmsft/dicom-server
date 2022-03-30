﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Dicom.Core.Features.Export;
public class ExportAzureStorage
{
    public string StorageAccountName { get; set; }

    public string StorageContainerName { get; set; }

    public string SasToken { get; set; }
}
