// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using EnsureThat;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Health.Api.Features.Audit;
using Microsoft.Health.Dicom.Api.Extensions;
using Microsoft.Health.Dicom.Api.Features.Filters;
using Microsoft.Health.Dicom.Api.Features.Routing;
using Microsoft.Health.Dicom.Core.Configs;
using Microsoft.Health.Dicom.Core.Exceptions;
using Microsoft.Health.Dicom.Core.Extensions;
using Microsoft.Health.Dicom.Core.Features.Audit;
using Microsoft.Health.Dicom.Core.Features.Crypto;
using Microsoft.Health.Dicom.Core.Messages.Export;
using Microsoft.Health.Dicom.Core.Models.Export;
using Microsoft.Health.Dicom.Core.Web;
using DicomAudit = Microsoft.Health.Dicom.Api.Features.Audit;

namespace Microsoft.Health.Dicom.Api.Controllers;

[ApiVersion("1.0-prerelease")]
[ApiVersion("1")]
[ServiceFilter(typeof(DicomAudit.AuditLoggingFilterAttribute))]
public class ExportController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ExportController> _logger;
    private readonly bool _featureEnabled;
    private readonly ICryptoService _cryptoService;

    public ExportController(
        IMediator mediator,
        IOptions<FeatureConfiguration> featureConfiguration,
        ILogger<ExportController> logger,
        ICryptoService cryptoService)

    {
        EnsureArg.IsNotNull(mediator, nameof(mediator));
        EnsureArg.IsNotNull(featureConfiguration?.Value, nameof(featureConfiguration));
        EnsureArg.IsNotNull(logger, nameof(logger));
        EnsureArg.IsNotNull(cryptoService, nameof(cryptoService));

        _mediator = mediator;
        _logger = logger;
        _featureEnabled = featureConfiguration.Value.EnableExport;
        _cryptoService = cryptoService;
    }

    [HttpPost]
    [BodyModelStateValidator]
    [Produces(KnownContentTypes.ApplicationJson)]
    [Consumes(KnownContentTypes.ApplicationJson)]
    [ProducesResponseType(typeof(ExportIdentifiersResponse), (int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [VersionedRoute(KnownRoutes.ExportRoute)]
    [Route(KnownRoutes.ExportRoute)]
    [AuditEventType(AuditEventSubType.Export)]
    public async Task<IActionResult> ExportIdentifiersAsync([Required][FromBody] ExportIdentifiersInput input)
    {
        EnsureArg.IsNotNull(input, nameof(input));
        _logger.LogInformation("DICOM Web Export request received to export '{Files}' to '{Sink}'.", input.Identifiers.Count, input.Destination.Type);

        EnsureFeatureIsEnabled();
        ExportIdentifiersResponse response = await _mediator.ExportIdentifiersAsync(input, HttpContext.RequestAborted);

        Response.AddLocationHeader(response.Operation.Href);
        return StatusCode((int)HttpStatusCode.Accepted, response.Operation);
    }


    [HttpGet]
    //[BodyModelStateValidator]
    //[Produces(KnownContentTypes.ApplicationJson)]
    //[Consumes(KnownContentTypes.ApplicationJson)]
    //[ProducesResponseType(typeof(ExportResponse), (int)HttpStatusCode.Accepted)]
    //[ProducesResponseType((int)HttpStatusCode.BadRequest)]
    //[VersionedRoute("encode")]
    [Route("encode")]
    [AuditEventType(AuditEventSubType.Export)]
    public async Task<string> Encode(string plainText)
    {
        _logger.LogInformation("DICOM Web Export encode request received, with input {PlainText}.", plainText);
        var res = await _cryptoService.EncryptString(plainText);
        await Task.Delay(1);
        return $"hello there {plainText} and encode is {res}";
    }


    private void EnsureFeatureIsEnabled()
    {
        if (!_featureEnabled)
        {
            throw new ExtendedQueryTagFeatureDisabledException();
        }
    }
}
