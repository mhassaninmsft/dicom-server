﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Dicom.Core.Features.Crypto;
using Microsoft.Health.Dicom.Core.Features.Operations;
using Microsoft.Health.Dicom.Core.Features.Routing;
using Microsoft.Health.Dicom.Core.Messages.Export;
using Microsoft.Health.Dicom.Core.Models.Export;
using Microsoft.Health.Operations;

namespace Microsoft.Health.Dicom.Core.Features.Export;

public class ExportService : IExportService
{
    private readonly ExportSourceFactory _sourceFactory;
    private readonly ExportSinkFactory _sinkFactory;
    private readonly IDicomOperationsClient _client;
    private readonly IUrlResolver _uriResolver;
    private readonly ISecretService _secretService;

    public ExportService(
        ExportSourceFactory sourceFactory,
        ExportSinkFactory sinkFactory,
        IDicomOperationsClient client,
        IUrlResolver uriResolver,
        ISecretService secretService)
    {
        _sourceFactory = EnsureArg.IsNotNull(sourceFactory, nameof(sourceFactory));
        _sinkFactory = EnsureArg.IsNotNull(sinkFactory, nameof(sinkFactory));
        _client = EnsureArg.IsNotNull(client, nameof(client));
        _uriResolver = EnsureArg.IsNotNull(uriResolver, nameof(uriResolver));
        _secretService = EnsureArg.IsNotNull(secretService, nameof(secretService));
    }

    /// <summary>
    /// Export.
    /// </summary>
    /// <param name="input">The export input.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response.</returns>
    public async Task<ExportIdentifiersResponse> StartExportingIdentifiersAsync(ExportIdentifiersInput input, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(input, nameof(input));

        OperationReference operation = await StartExportAsync(
            new ExportInput
            {
                // TODO: Add batching options
                Manifest = new SourceManifest
                {
                    Input = input.Identifiers,
                    Type = ExportSourceType.Identifiers,
                },
                Destination = input.Destination,
                Batching = new Models.Operations.BatchOptions
                {
                    MaxParallel = 1,
                    Size = 1,
                }
            },
            cancellationToken);

        return new ExportIdentifiersResponse(operation);
    }

    private async Task<OperationReference> StartExportAsync(ExportInput input, CancellationToken cancellationToken)
    {
        _sourceFactory.Validate(input.Manifest);
        _sinkFactory.Validate(input.Destination);
        await _sinkFactory.EncryptSecrets(input.Destination);

        Guid operationId = await _client.StartExportAsync(input, cancellationToken);
        return new OperationReference(operationId, _uriResolver.ResolveOperationStatusUri(operationId));
    }
}
