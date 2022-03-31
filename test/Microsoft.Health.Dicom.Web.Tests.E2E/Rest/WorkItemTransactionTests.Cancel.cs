// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FellowOakDicom;
using Microsoft.Health.Dicom.Client;
using Microsoft.Health.Dicom.Core;
using Microsoft.Health.Dicom.Core.Features.Workitem;
using Microsoft.Health.Dicom.Core.Features.Workitem.Model;
using Microsoft.Health.Dicom.Tests.Common;
using Xunit;

namespace Microsoft.Health.Dicom.Web.Tests.E2E.Rest;

public partial class WorkItemTransactionTests
{
    [Fact]
    [Trait("Category", "bvt-fe")]
    public async Task GivenCancelWorkitem_WhenWorkitemWasNeverCanceledOrCompleted_TheServerShouldCancelWorkitemSuccessfully()
    {
        var workitemUid = TestUidGenerator.Generate();
        var patientName = $"TestUser-{workitemUid}";

        // Create
        var dicomDataset = Samples.CreateRandomWorkitemInstanceDataset(workitemUid);
        dicomDataset.AddOrUpdate(DicomTag.PatientName, patientName);

        using var addResponse = await _client.AddWorkitemAsync(Enumerable.Repeat(dicomDataset, 1), workitemUid);
        Assert.True(addResponse.IsSuccessStatusCode);

        // Cancel
        var cancelDicomDataset = Samples.CreateWorkitemCancelRequestDataset(@"Test Cancel");
        using var cancelResponse = await _client.CancelWorkitemAsync(Enumerable.Repeat(cancelDicomDataset, 1), workitemUid);
        Assert.True(cancelResponse.IsSuccessStatusCode);

        // Query
        using var queryResponse = await _client.QueryWorkitemAsync($"PatientName={patientName}");
        var responseDatasets = await queryResponse.ToArrayAsync();
        var actualDataset = responseDatasets?.FirstOrDefault();

        // Verify
        Assert.NotNull(actualDataset);
        Assert.Equal(workitemUid, actualDataset.GetSingleValue<string>(DicomTag.SOPInstanceUID));
        Assert.Equal(ProcedureStepState.Canceled, ProcedureStepStateExtensions.GetProcedureState(actualDataset));
    }

    [Fact]
    public async Task GivenCancelWorkitem_WhenWorkitemWasAlreadyCanceled_ServerShouldReturn409()
    {
        var workitemUid = TestUidGenerator.Generate();
        var patientName = $"TestUser-{workitemUid}";

        // Create
        var dicomDataset = Samples.CreateRandomWorkitemInstanceDataset(workitemUid);
        dicomDataset.AddOrUpdate(DicomTag.PatientName, patientName);

        using var addResponse = await _client.AddWorkitemAsync(Enumerable.Repeat(dicomDataset, 1), workitemUid);
        Assert.True(addResponse.IsSuccessStatusCode);

        // Cancel
        var cancelDicomDataset = Samples.CreateWorkitemCancelRequestDataset(@"Test Cancel");
        using var cancelResponse1 = await _client.CancelWorkitemAsync(Enumerable.Repeat(cancelDicomDataset, 1), workitemUid);
        Assert.True(cancelResponse1.IsSuccessStatusCode);

        // Cancel
        var exception = await Assert.ThrowsAsync<DicomWebException>(() => _client.CancelWorkitemAsync(Enumerable.Repeat(cancelDicomDataset, 1), workitemUid));

        // Verify
        Assert.Equal("\"" + string.Format(DicomCoreResource.WorkitemIsAlreadyCanceled, workitemUid) + "\"", exception.ResponseMessage);
        Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
    }

    [Fact]
    public async Task GivenCancelWorkitem_WhenWorkitemIsNotFound_ServerShouldReturn404()
    {
        var workitemUid = TestUidGenerator.Generate();
        var patientName = $"TestUser-{workitemUid}";

        // Create
        var dicomDataset = Samples.CreateRandomWorkitemInstanceDataset(workitemUid);
        dicomDataset.AddOrUpdate(DicomTag.PatientName, patientName);

        using var addResponse = await _client.AddWorkitemAsync(Enumerable.Repeat(dicomDataset, 1), workitemUid);
        Assert.True(addResponse.IsSuccessStatusCode);

        // Cancel
        var newWorkitemUid = TestUidGenerator.Generate();
        var cancelDicomDataset = Samples.CreateWorkitemCancelRequestDataset(@"Test Cancel");
        var exception = await Assert.ThrowsAsync<DicomWebException>(() => _client.CancelWorkitemAsync(Enumerable.Repeat(cancelDicomDataset, 1), newWorkitemUid));

        // Verify
        Assert.Equal("\"" + string.Format(DicomCoreResource.WorkitemInstanceNotFound, newWorkitemUid) + "\"", exception.ResponseMessage);
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}
