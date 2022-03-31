// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------



using System.Threading.Tasks;

namespace Microsoft.Health.Dicom.Core.Features.Crypto;
public interface ISecretService
{
    Task<SecretRepresntation> StoreSecret(string secretName, string secretValue);
    Task<string> GetSecret(string secretName);

}
