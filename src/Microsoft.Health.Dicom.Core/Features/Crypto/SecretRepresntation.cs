// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;


namespace Microsoft.Health.Dicom.Core.Features.Crypto;
public class SecretRepresntation
{
    public DateTimeOffset ExpiryTime { get; set; }
    public string SecretName { get; set; }
    /// <summary>
    /// Optional, in case the secret has an id
    /// </summary>
    public string SecretId { get; set; }
}
