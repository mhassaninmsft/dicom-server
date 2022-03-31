// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------


//using Microsoft.Extensions.Logging;

//using System;
using System.Threading.Tasks;
using Azure.Core;
//using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.Health.Dicom.Core.Features.Crypto;
public class AzureKeyVaultCryptoService : ISecretService
{
    // Constant for now, TODO, change via either a constructor and use DI
    private const int SecretStorageDurationHours = 48; // 2 days
    private readonly ILogger<AzureKeyVaultCryptoService> _logger;
    private readonly TokenCredential _tokenCredential;
    //private readonly CryptographyClient _cryptographyClient;
    private readonly AzureKeyVaultConfig _azureKeyVaultConfig;
    private readonly SecretClient _secretClient;
    public AzureKeyVaultCryptoService(ILogger<AzureKeyVaultCryptoService> logger, IOptions<AzureKeyVaultConfig> keyVaultConfig, TokenCredential keyVaultTokenCredential)
    {
        EnsureArg.IsNotNull(logger, nameof(logger));
        EnsureArg.IsNotNull(keyVaultTokenCredential, nameof(keyVaultTokenCredential));
        EnsureArg.IsNotNull(keyVaultConfig?.Value, nameof(keyVaultConfig));
        _logger = logger;
        _tokenCredential = keyVaultTokenCredential;
        _azureKeyVaultConfig = keyVaultConfig.Value;

        // for storing secrets
        var secretClient = new SecretClient(_azureKeyVaultConfig.VaultUri, keyVaultTokenCredential);
        _secretClient = secretClient;
    }



    public async Task<string> GetSecret(string secretName)
    {
        var secretValue = await _secretClient.GetSecretAsync(secretName);
        return secretValue.Value.Value;
    }

    private static DateTimeOffset GetExpiryTime()
    {
        return new DateTimeOffset(DateTime.Now + TimeSpan.FromHours(SecretStorageDurationHours));
    }
    public async Task<SecretRepresntation> StoreSecret(string secretName, string secretValue)
    {
        KeyVaultSecret secretObject = new KeyVaultSecret(secretName, secretValue);
        var expiryTime = GetExpiryTime();
        secretObject.Properties.ExpiresOn = expiryTime;
        var keyVaultSecret = (await _secretClient.SetSecretAsync(secretName, secretValue)).Value;
        var result = new SecretRepresntation() { ExpiryTime = expiryTime, SecretName = secretName, SecretId = keyVaultSecret.Id.ToString() };
        return result;
    }
}
