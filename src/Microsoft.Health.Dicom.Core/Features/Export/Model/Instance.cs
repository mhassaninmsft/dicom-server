// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------


namespace Microsoft.Health.Dicom.Core.Features.Export.Model;
public readonly record struct Instance(string StudyId, string SeriesId, string InstanceId);
public readonly record struct Series(string StudyId, string SeriesId);
public readonly record struct Study(string StudyId);

//public record Instance
//{
//    public string InstanceId { get; set; }
//    public string SeriesId { get; set; }
//    public string StudyId { get; set; }

//}
//public record Series
//{
//    public string SeriesId { get; set; }
//    public string StudyId { get; set; }

//}

//public record Study
//{
//    public string StudyId { get; set; }

//}

