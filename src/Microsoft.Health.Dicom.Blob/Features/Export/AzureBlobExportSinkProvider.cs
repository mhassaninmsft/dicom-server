// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using System.Text;
using Azure.Storage.Blobs;
using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Health.Blob.Configs;
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

    public IExportSink Create(IServiceProvider provider, IConfiguration config, Guid operationId)

    {
        EnsureArg.IsNotNull(provider, nameof(provider));
        EnsureArg.IsNotNull(config, nameof(config));
        // source objects
        var sourceClient = provider.GetRequiredService<BlobServiceClient>();
        var blobOptions = provider.GetRequiredService<IOptions<BlobOperationOptions>>();
        var blobContainerConfig = provider.GetRequiredService<IOptionsMonitor<BlobContainerConfiguration>>();
        DecrypSecrets(config).Wait();
        // destination objects
        InitializeDestinationStore(config, out BlobContainerClient destClient, out string destPath);

        // init and return
        return new AzureBlobExportSink(
            sourceClient,
            destClient,
            Encoding.UTF8,
            destPath,
            $"error-{operationId:N}.log",
            blobContainerConfig,
            blobOptions);
    }

    public void Validate(IConfiguration config)
    {
        AzureBlobExportOptions options = config.Get<AzureBlobExportOptions>();

        if (options.ContainerUri == null)
            throw new FormatException();

        // todo config length
        // Valid names https://docs.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata
        if (!string.IsNullOrWhiteSpace(options.FolderPath) && options.FolderPath.Length > 200)
        {
            throw new ArgumentException("Folder path too long");
        }
    }

    public async Task EncrypSecrets(IConfiguration config)
    {
        EnsureArg.IsNotNull(config, nameof(config));
        AzureBlobExportOptions options = config.Get<AzureBlobExportOptions>();
        if (options.SasToken != null)
        {
            var sasToken = options.SasToken;
            var secretName = Guid.NewGuid().ToString();
            await _secretService.StoreSecret(secretName, sasToken);
            config["AzureBlobExportOptions:SasToken"] = secretName;
        }

    }

    public async Task DecrypSecrets(IConfiguration config)
    {
        EnsureArg.IsNotNull(config, nameof(config));
        AzureBlobExportOptions options = config.Get<AzureBlobExportOptions>();
        if (!string.IsNullOrEmpty(options.SasToken))
        {
            var secretName = options.SasToken;
            var decryptedSasToken = await _secretService.GetSecret(secretName);
            config["AzureBlobExportOptions:SasToken"] = decryptedSasToken;
        }

    }

    private static void InitializeDestinationStore(IConfiguration config, out BlobContainerClient blobContainerClient, out string path)
    {
        var blobClientOptions = config.Get<BlobServiceClientOptions>();
        var exportOptions = config.Get<AzureBlobExportOptions>();

        path = exportOptions.FolderPath;

        if (exportOptions.SasToken == null)
        {
            throw new NotImplementedException();
            //need a way to pass the MI config from KeyVault to here
            //DefaultAzureCredential credential = new DefaultAzureCredential(blobClientOptions.Credentials);
            //blobContainerClient = new BlobContainerClient(exportConfig.ContainerUri, credential, blobClientOptions);
        }
        else
        {
            var builder = new UriBuilder(exportOptions.ContainerUri);
            builder.Query += exportOptions.SasToken;

            blobContainerClient = new BlobContainerClient(builder.Uri, blobClientOptions);
        }
    }
}
