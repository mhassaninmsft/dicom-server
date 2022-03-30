// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Dicom.Core.Features.Export;
using Microsoft.Health.Dicom.Core.Features.Export.Model;
//using Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Health.Dicom.Core.UnitTests.Features.Export;
public class ExportOperationTests
{
    private readonly ITestOutputHelper _output;

    public ExportOperationTests(ITestOutputHelper output)
    {
        _output = output;
    }
    [Fact]
    public void BasicInput()
    {
        Console.WriteLine("Hello");
        _output.WriteLine("Hello23");
        var ids = new string[] { "123", "323/123", "1111/22/3333" };
        var input = new ExportInput()
        {
            Source = new ExportSource()
            {
                IdFilter = new ExportIdFilter() { Ids = ids }
            }
        };
        var res1 = ExportService.GetOperationInput(input);
        var series1 = new Series("323", "123");
        Assert.Contains(series1, res1.Source.Series);
        _output.WriteLine(res1.Source.ToString());
        Assert.Equal(5, 5);
    }
}
