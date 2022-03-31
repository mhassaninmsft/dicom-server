// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------


//using Microsoft.Extensions.Logging;

//using System;
using System.Threading.Tasks;
using Azure.Core;
//using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Health.Dicom.Core.Features.Crypto;
public class AzureKeyVaultCryptoService : ICryptoService
{
    private readonly ILogger<AzureKeyVaultCryptoService> _logger;
    private readonly TokenCredential _tokenCredential;
    private readonly CryptographyClient _cryptographyClient;
    private readonly AzureKeyVaultConfig _azureKeyVaultConfig;
    public AzureKeyVaultCryptoService(ILogger<AzureKeyVaultCryptoService> logger, IOptions<AzureKeyVaultConfig> keyVaultConfig, TokenCredential keyVaultTokenCredential)
    {
        EnsureArg.IsNotNull(logger, nameof(logger));
        EnsureArg.IsNotNull(keyVaultTokenCredential, nameof(keyVaultTokenCredential));
        EnsureArg.IsNotNull(keyVaultConfig?.Value, nameof(keyVaultConfig));
        _logger = logger;
        _tokenCredential = keyVaultTokenCredential;
        _azureKeyVaultConfig = keyVaultConfig.Value;

        var vaultClient = new KeyClient(_azureKeyVaultConfig.KeyVaultUri, _tokenCredential);
        var key = vaultClient.GetKeyAsync(_azureKeyVaultConfig.KeyName).Result.Value;
        var cryptoClient = new CryptographyClient(key.Id, keyVaultTokenCredential);
        _cryptographyClient = cryptoClient;

    }

    public async Task<byte[]> Decrypt(byte[] data)
    {
        DecryptResult decryptResult = await _cryptographyClient.DecryptAsync(EncryptionAlgorithm.RsaOaep, data);
        return decryptResult.Plaintext;
    }

    public async Task<byte[]> Encrypt(byte[] data)
    {
        EncryptResult encryptResult = await _cryptographyClient.EncryptAsync(EncryptionAlgorithm.RsaOaep, data);
        return encryptResult.Ciphertext;
    }
}
