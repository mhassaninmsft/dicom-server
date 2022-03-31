﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------
using System;
namespace Microsoft.Health.Dicom.Core.Features.Crypto;
public class AzureKeyVaultConfig
{
    public Uri VaultUri { get; set; }
    public string KeyName { get; set; }
}
