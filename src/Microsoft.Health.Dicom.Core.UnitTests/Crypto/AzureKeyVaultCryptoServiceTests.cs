// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Dicom.Core.Crypto;
using NSubstitute;
using Xunit;
using System;
using Xunit.Abstractions;
using Microsoft.Extensions.Options;
using Azure.Identity;

namespace Microsoft.Health.Dicom.Core.UnitTests.Crypto;
public class AzureKeyVaultCryptoServiceTests
{
    private readonly ITestOutputHelper _output;

    public AzureKeyVaultCryptoServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }
    [Fact]
    public async Task TestBaseWorkflow()
    {
        var logger = Substitute.For<ILogger<AzureKeyVaultCryptoService>>();
        var keyVaultConfig = new AzureKeyVaultConfig() { KeyName = "mynewkey", KeyVaultUri = new Uri("https://dicomkeyvault1.vault.azure.net/") };
        var keyVaultConfigOptions = Options.Create(keyVaultConfig);
        var tenantID = "88f738c6-baab-45a8-b695-ee3cadd61660";
        var clientID = "82887e4c-cb3f-465a-aab9-bbbee719a2f1";
        var clientSecret = "-GY7Q~i7bsCWj~j6kP5KzJKu~RjXsDL5JI_fb";
        var clientSecretCredential = new ClientSecretCredential(tenantID, clientID, clientSecret);
        ICryptoService cryptoService = new AzureKeyVaultCryptoService(logger, keyVaultConfigOptions, clientSecretCredential);
        var secretString = "This is a seceret2";
        var encryptedString = await cryptoService.EncryptString(secretString);
        var plainString = await cryptoService.DecryptString(encryptedString);
        _output.WriteLine(plainString);
        _output.WriteLine(encryptedString);
        Assert.Equal(secretString, plainString);
    }
}
