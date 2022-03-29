// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Dicom.Core.Features.Export;
using Microsoft.Health.Dicom.Core.Features.Operations;
using Microsoft.Health.Dicom.Core.Features.Routing;
using Microsoft.Health.Dicom.Core.Messages.Export;
using Microsoft.Health.Operations;
using System.Linq;
using Microsoft.Health.Dicom.Core.Features.Export.Model;

namespace Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;

public class ExportService : IExportService
{

    private readonly IDicomOperationsClient _client;
    private readonly IUrlResolver _uriResolver;

    public ExportService(IDicomOperationsClient client, IUrlResolver uriResolver)
    {
        _client = EnsureArg.IsNotNull(client, nameof(client));
        _uriResolver = EnsureArg.IsNotNull(uriResolver, nameof(uriResolver));
    }

    /// <summary>
    /// Add Extended Query Tags.
    /// </summary>
    /// <param name="exportInput">The export input.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response.</returns>
    public async Task<ExportResponse> ExportAsync(ExportInput exportInput, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(exportInput, nameof(exportInput));
        Guid operationId = await _client.StartExportAsync(GetOperationInput(exportInput), cancellationToken);
        return new ExportResponse(new OperationReference(operationId, _uriResolver.ResolveOperationStatusUri(operationId)));
    }

    public static ExportOperationInput GetOperationInput(ExportInput input)
    {
        EnsureArg.IsNotNull(input, nameof(input));
        // TODO: inmplement this method
        var separatorCharacter = '/';
        var instances = new HashSet<Instance>();
        var series = new HashSet<Series>();
        var studies = new HashSet<Study>();
        foreach (var id in input.Source.IdFilter.Ids)
        {
            var numForwardSlashes = id.Count(c => c == separatorCharacter);
            var splitted = id.Split(separatorCharacter);
            switch (numForwardSlashes)
            {
                case 0: // Study
                    var studyId = splitted[0];
                    studies.Add(new Study(studyId));
                    break;
                case 1: // Series
                    studyId = splitted[0];
                    var seriesId = splitted[1];
                    series.Add(new Series(studyId, seriesId));
                    break;
                case 2:  //Instance
                    studyId = splitted[0];
                    seriesId = splitted[1];
                    var instanceId = splitted[2];
                    instances.Add(new Instance(studyId, seriesId, instanceId));
                    break;
                default:
                    break;
            }

        }
        var exportOperationInput = new ExportOperationInput()
        {
            Destination = input.Destination,
            Source = new ExportOperationSource()
            {
                Instances = instances,
                Series = series,
                Studies = studies,
            }
        };
        return exportOperationInput;

    }
}
