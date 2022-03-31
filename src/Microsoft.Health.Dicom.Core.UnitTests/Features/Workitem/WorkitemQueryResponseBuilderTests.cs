﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using FellowOakDicom;
using Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;
//using Microsoft.Health.Dicom.Core.Features.Query;
using Microsoft.Health.Dicom.Core.Features.Query.Model;
using Microsoft.Health.Dicom.Core.Features.Query.Model.FilterConditions;
using Microsoft.Health.Dicom.Core.Features.Workitem;
using Microsoft.Health.Dicom.Tests.Common;
using Xunit;

namespace Microsoft.Health.Dicom.Core.UnitTests.Features.Workitem;

public class WorkitemQueryResponseBuilderTests
{
    [Fact]
    public void GivenWorkitem_WithIncludeField_ValidReturned()
    {
        var includeField = new QueryIncludeField(new List<DicomTag> { DicomTag.WorklistLabel });
        var filters = new List<QueryFilterCondition>()
        {
            new StringSingleValueMatchCondition(new QueryTag(DicomTag.PatientName), "Foo"),
        };
        var query = new BaseQueryExpression(includeField, false, 0, 0, filters);
        var dataset = Samples
            .CreateRandomWorkitemInstanceDataset()
            .AddOrUpdate(new DicomLongString(DicomTag.MedicalAlerts))
            .AddOrUpdate(new DicomShortString(DicomTag.SnoutID));

        DicomDataset responseDataset = WorkitemQueryResponseBuilder.GenerateResponseDataset(dataset, query);
        var tags = responseDataset.Select(i => i.Tag).ToList();

        Assert.Contains(DicomTag.WorklistLabel, tags); // Valid include
        Assert.Contains(DicomTag.PatientName, tags); // Valid filter
        Assert.Contains(DicomTag.MedicalAlerts, tags); // Required return attribute

        Assert.DoesNotContain(DicomTag.TransactionUID, tags); // should never be included
        Assert.DoesNotContain(DicomTag.SnoutID, tags); // Not a required return attribute
    }

    [Fact]
    public void GivenWorkitem_WithIncludeFieldAll_AllReturned()
    {
        var includeField = QueryIncludeField.AllFields;
        var filters = new List<QueryFilterCondition>();
        var query = new BaseQueryExpression(includeField, false, 0, 0, filters);
        var dataset = Samples
            .CreateRandomWorkitemInstanceDataset()
            .AddOrUpdate(new DicomLongString(DicomTag.MedicalAlerts))
            .AddOrUpdate(new DicomShortString(DicomTag.SnoutID));

        DicomDataset responseDataset = WorkitemQueryResponseBuilder.GenerateResponseDataset(dataset, query);
        var tags = responseDataset.Select(i => i.Tag).ToList();

        Assert.Contains(DicomTag.MedicalAlerts, tags); // Required return attribute
        Assert.Contains(DicomTag.SnoutID, tags); // Not a required return attribute - set by 'all'

        Assert.DoesNotContain(DicomTag.TransactionUID, tags); // should never be included
    }
}
