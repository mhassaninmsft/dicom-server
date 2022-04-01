// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Health.Dicom.Core.Features.Crypto;
using NSubstitute;
using Xunit;
using System;
using Xunit.Abstractions;
using Microsoft.Extensions.Options;
using Azure.Identity;
using System.Threading.Tasks;

namespace Microsoft.Health.Dicom.Core.UnitTests.Features.Crypto;
public class AzureKeyVaultCryptoServiceTests
{
    private readonly ITestOutputHelper _output;

    public AzureKeyVaultCryptoServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }
    //TODO: Move to integration tests
    [Fact(Skip = "Should be in integration tests, since it requires a connection to an Azure Key vault")]
    public async Task TestBaseWorkflow()
    {
        var logger = Substitute.For<ILogger<AzureKeyVaultCryptoService>>();
        var keyVaultConfig = new AzureKeyVaultConfig() { VaultUri = new Uri("https://dicomkeyvault1.vault.azure.net/") };
        var keyVaultConfigOptions = Options.Create(keyVaultConfig);
        var tenantID = "88f738c6-baab-45a8-b695-ee3cadd61660";
        var clientID = "82887e4c-cb3f-465a-aab9-bbbee719a2f1";
        var clientSecret = "";
        var clientSecretCredential = new ClientSecretCredential(tenantID, clientID, clientSecret);
        ISecretService secretService = new AzureKeyVaultCryptoService(logger, keyVaultConfigOptions, clientSecretCredential);
        var secretString = "This is a seceret2";
        var secretName = "mySuperSecret";
        var secret = await secretService.StoreSecret(secretName, secretString);
        var plainSecret = await secretService.GetSecret(secretName);
        Assert.Equal(plainSecret, secretString);
    }
}
