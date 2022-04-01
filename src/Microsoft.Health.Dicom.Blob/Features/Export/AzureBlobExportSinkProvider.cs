// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Health.Blob.Configs;
using Microsoft.Health.Dicom.Blob.Features.Storage;
using Microsoft.Health.Dicom.Core.Features.Crypto;
using Microsoft.Health.Dicom.Core.Features.Export;
using Microsoft.Health.Dicom.Core.Models.Export;

namespace Microsoft.Health.Dicom.Blob.Features.Export;

public class AzureBlobExportSinkProvider : IExportSinkProvider
{
    private readonly ISecretService _secretService;
    public ExportDestinationType Type => ExportDestinationType.AzureBlob;
    public AzureBlobExportSinkProvider(ISecretService secretService)
    {
        EnsureArg.IsNotNull(secretService, nameof(secretService));
        _secretService = secretService;
    }
    public IExportSink Create(IServiceProvider provider, IConfiguration config)
    {
        EnsureArg.IsNotNull(provider, nameof(provider));
        EnsureArg.IsNotNull(config, nameof(config));
        // source objects
        var sourceBlobServiceClient = provider.GetService<BlobServiceClient>();
        var blobOptions = provider.GetService<IOptions<BlobOperationOptions>>();
        var blobContainerConfig = provider.GetService<IOptionsMonitor<BlobContainerConfiguration>>();
        // The Function App needs to register an Azure KeyVault service
        //ISecretService secretService = provider.GetService<ISecretService>();
        //config = SubstituteSASTokenSecretIfExists(secretService, config).Result;
        DecrypSecrets(config).Wait();
        // destination objects
        InitializeDestinationStore(config, out BlobContainerClient destBlobContainerClient, out string destPath);

        // init and return
        BlobCopyStore store = new BlobCopyStore(sourceBlobServiceClient, blobContainerConfig, blobOptions, destBlobContainerClient, destPath);
        return new AzureBlobExportSink(store);
    }

    public void Validate(IConfiguration config)
    {
        AzureBlobExportOptions options = config.Get<AzureBlobExportOptions>();

        if (options.ContainerUri != null && options.ContainerSasUri != null)
            throw new FormatException();
        else if (options.ContainerUri == null && options.ContainerSasUri == null)
            throw new FormatException();
    }

    public async Task EncrypSecrets(IConfiguration config)
    {
        EnsureArg.IsNotNull(config, nameof(config));
        AzureBlobExportOptions options = config.Get<AzureBlobExportOptions>();
        if (options.ContainerSasUri != null)
        {
            var containerSasUri = options.ContainerSasUri.ToString();
            var secretName = Guid.NewGuid().ToString();
            await _secretService.StoreSecret(secretName, containerSasUri);
            config["AzureBlobExportOptions:SasTokenName"] = secretName;
            // TODO: IS there a better way of doing this
            config["AzureBlobExportOptions:ContainerSasUri"] = "https://www.ununsed.com";
        }

    }

    public async Task DecrypSecrets(IConfiguration config)
    {
        EnsureArg.IsNotNull(config, nameof(config));
        AzureBlobExportOptions options = config.Get<AzureBlobExportOptions>();
        if (!string.IsNullOrEmpty(options.SasTokenName))
        {
            var secretName = options.SasTokenName;
            var sasTokenUri = await _secretService.GetSecret(secretName);
            config["AzureBlobExportOptions:SasTokenName"] = "";
            // TODO: IS there a better way of doing this
            config["AzureBlobExportOptions:ContainerSasUri"] = sasTokenUri;

        }

    }

    private static void InitializeDestinationStore(IConfiguration config, out BlobContainerClient blobContainerClient, out string path)
    {
        var blobClientOptions = config.Get<BlobServiceClientOptions>();
        var exportConfig = config.Get<AzureBlobExportOptions>();

        path = exportConfig.FolderPath;

        if (exportConfig.ContainerUri != null)
        {
            throw new NotImplementedException();
            //need a way to pass the MI config from KeyVault to here
            //DefaultAzureCredential credential = new DefaultAzureCredential(blobClientOptions.Credentials);
            //blobContainerClient = new BlobContainerClient(exportConfig.ContainerUri, credential, blobClientOptions);
        }
        else
        {
            blobContainerClient = new BlobContainerClient(exportConfig.ContainerSasUri, blobClientOptions);
        }
    }
}
