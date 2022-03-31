﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using EnsureThat;
using FellowOakDicom;
using Microsoft.Health.Dicom.Core.Exceptions;
using Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;
using Microsoft.Health.Dicom.Core.Features.Model;
using Microsoft.Health.Dicom.Core.Features.Store;
using Microsoft.Health.Dicom.Core.Features.Validation;
using Microsoft.Health.Dicom.Core.Features.Workitem.Model;
using Microsoft.Health.Dicom.Core.Models;

namespace Microsoft.Health.Dicom.Core.Extensions;

/// <summary>
/// Extension methods for <see cref="DicomDataset"/>.
/// </summary>
public static class DicomDatasetExtensions
{
    private static readonly HashSet<DicomVR> DicomBulkDataVr = new HashSet<DicomVR>
    {
        DicomVR.OB,
        DicomVR.OD,
        DicomVR.OF,
        DicomVR.OL,
        DicomVR.OV,
        DicomVR.OW,
        DicomVR.UN,
    };

    private const string DateFormatDA = "yyyyMMdd";

    private static readonly string[] DateTimeFormatsDT =
    {
        "yyyyMMddHHmmss.FFFFFFzzz",
        "yyyyMMddHHmmsszzz",
        "yyyyMMddHHmmzzz",
        "yyyyMMddHHzzz",
        "yyyyMMddzzz",
        "yyyyMMzzz",
        "yyyyzzz",
        "yyyyMMddHHmmss.FFFFFF",
        "yyyyMMddHHmmss",
        "yyyyMMddHHmm",
        "yyyyMMddHH",
        "yyyyMMdd",
        "yyyyMM",
        "yyyy"
    };

    private static readonly string[] DateTimeOffsetFormats = new string[]
    {
        "hhmm",
        "\\+hhmm",
        "\\-hhmm"
    };

    /// <summary>
    /// Gets a single value if the value exists; otherwise the default value for the type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="dicomDataset">The dataset to get the VR value from.</param>
    /// <param name="dicomTag">The DICOM tag.</param>
    /// <param name="expectedVR">Expected VR of the element.</param>
    /// <remarks>If expectedVR is provided, and not match, will return default<typeparamref name="T"/></remarks>
    /// <returns>The value if the value exists; otherwise, the default value for the type <typeparamref name="T"/>.</returns>
    public static T GetSingleValueOrDefault<T>(this DicomDataset dicomDataset, DicomTag dicomTag, DicomVR expectedVR = null)
    {
        EnsureArg.IsNotNull(dicomDataset, nameof(dicomDataset));

        // If VR doesn't match, return default(T)
        if (expectedVR != null && dicomDataset.GetDicomItem<DicomElement>(dicomTag)?.ValueRepresentation != expectedVR)
        {
            return default;
        }

        return dicomDataset.GetSingleValueOrDefault<T>(dicomTag, default);
    }

    /// <summary>
    /// Gets the DA VR value as <see cref="DateTime"/>.
    /// </summary>
    /// <param name="dicomDataset">The dataset to get the VR value from.</param>
    /// <param name="dicomTag">The DICOM tag.</param>
    /// <param name="expectedVR">Expected VR of the element.</param>
    /// <remarks>If expectedVR is provided, and not match, will return null.</remarks>
    /// <returns>An instance of <see cref="DateTime"/> if the value exists and comforms to the DA format; otherwise <c>null</c>.</returns>
    public static DateTime? GetStringDateAsDate(this DicomDataset dicomDataset, DicomTag dicomTag, DicomVR expectedVR = null)
    {
        EnsureArg.IsNotNull(dicomDataset, nameof(dicomDataset));
        string stringDate = dicomDataset.GetSingleValueOrDefault<string>(dicomTag, expectedVR: expectedVR);
        return DateTime.TryParseExact(stringDate, DateFormatDA, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result) ? result : null;
    }

    /// <summary>
    /// Gets the DT VR values as literal <see cref="DateTime"/> and UTC <see cref="DateTime"/>.
    /// If offset is not provided in the value and in the TimezoneOffsetFromUTC fields, UTC DateTime will be null.
    /// </summary>
    /// <param name="dicomDataset">The dataset to get the VR value from.</param>
    /// <param name="dicomTag">The DICOM tag.</param>
    /// <param name="expectedVR">Expected VR of the element.</param>
    /// <remarks>If expectedVR is provided, and not match, will return null.</remarks>
    /// <returns>A <see cref="Tuple{T1, T2}"/> of (<see cref="Nullable{T}"/> <see cref="DateTime"/>, <see cref="Nullable{T}"/> <see cref="DateTime"/>) representing literal date time and Utc date time respectively is returned. If value does not exist or does not conform to the DT format, <c>null</c> is returned for DateTimes. If offset information is not present, <c>null</c> is returned for Item2 i.e. Utc Date Time.</returns>
    public static Tuple<DateTime?, DateTime?> GetStringDateTimeAsLiteralAndUtcDateTimes(this DicomDataset dicomDataset, DicomTag dicomTag, DicomVR expectedVR = null)
    {
        EnsureArg.IsNotNull(dicomDataset, nameof(dicomDataset));
        string stringDateTime = dicomDataset.GetSingleValueOrDefault<string>(dicomTag, expectedVR: expectedVR);

        if (string.IsNullOrEmpty(stringDateTime))
        {
            // If no valid data found, return null values in tuple.
            return new Tuple<DateTime?, DateTime?>(null, null);
        }

        Tuple<DateTime?, DateTime?> result = new Tuple<DateTime?, DateTime?>(null, null);

        // Parsing as DateTime such that we can know the DateTimeKind.
        // If offset is present in the value, DateTimeKind is Local, else it is Unspecified.
        // Ideally would like to work with just DateTimeOffsets to avoid parsing multiple times, but DateTimeKind does not work as expected with DateTimeOffset.
        if (DateTime.TryParseExact(stringDateTime, DateTimeFormatsDT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
        {
            // Using DateTimeStyles.AssumeUniversal here such that when applying offset, local timezone (offset) is not taken into account.
            DateTimeOffset.TryParseExact(stringDateTime, DateTimeFormatsDT, null, DateTimeStyles.AssumeUniversal, out DateTimeOffset dateTimeOffset);

            // Unspecified means that the offset is not present in the value.
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                // Check if offset is present in TimezoneOffsetFromUTC
                TimeSpan? offset = dicomDataset.GetTimezoneOffsetFromUtcAsTimeSpan();

                if (offset != null)
                {
                    // If we can parse the offset, use that offset to calculate UTC Date Time.
                    result = new Tuple<DateTime?, DateTime?>(dateTimeOffset.DateTime, dateTimeOffset.ToOffset(offset.Value).DateTime);
                }
                else
                {
                    // If either offset is not present or could not be parsed, UTC should be null.
                    result = new Tuple<DateTime?, DateTime?>(dateTimeOffset.DateTime, null);
                }
            }
            else
            {
                // If offset is present in the value, it can simply be converted to UTC
                result = new Tuple<DateTime?, DateTime?>(dateTimeOffset.DateTime, dateTimeOffset.UtcDateTime);
            }
        }

        return result;
    }

    /// <summary>
    /// Get TimezoneOffsetFromUTC value as TimeSpan.
    /// </summary>
    /// <param name="dicomDataset">The dataset to get the TimezoneOffsetFromUTC value from.</param>
    /// <returns>An instance of <see cref="Nullable"/> <see cref="TimeSpan"/>. If value is not found or could not be parsed, <c>null</c> is returned.</returns>
    private static TimeSpan? GetTimezoneOffsetFromUtcAsTimeSpan(this DicomDataset dicomDataset)
    {
        // Cannot parse it directly as TimeSpan as the offset needs to follow specific formats.
        string offset = dicomDataset.GetSingleValueOrDefault<string>(DicomTag.TimezoneOffsetFromUTC, expectedVR: DicomVR.SH);

        if (!string.IsNullOrEmpty(offset))
        {
            TimeSpan timeSpan;

            // Need to look at offset string to figure out positive or negative offset
            // as timespan ParseExact does not support negative offsets by default.
            // Applying TimeSpanStyles.AssumeNegative is the only documented way to handle negative offsets for this method.
            bool isSuccess = offset[0] == '-' ?
                TimeSpan.TryParseExact(offset, DateTimeOffsetFormats, CultureInfo.InvariantCulture, TimeSpanStyles.AssumeNegative, out timeSpan) :
                TimeSpan.TryParseExact(offset, DateTimeOffsetFormats, CultureInfo.InvariantCulture, out timeSpan);

            if (isSuccess)
            {
                return timeSpan;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the TM VR value as <see cref="long"/>.
    /// </summary>
    /// <param name="dicomDataset">The dataset to get the VR value from.</param>
    /// <param name="dicomTag">The DICOM tag.</param>
    /// <param name="expectedVR">Expected VR of the element.</param>
    /// <remarks>If expectedVR is provided, and not match, will return null.</remarks>
    /// <returns>A long value representing the ticks if the value exists and conforms to the TM format; othewise <c>null</c>.</returns>
    public static long? GetStringTimeAsLong(this DicomDataset dicomDataset, DicomTag dicomTag, DicomVR expectedVR = null)
    {
        EnsureArg.IsNotNull(dicomDataset, nameof(dicomDataset));
        long? result = null;

        try
        {
            result = dicomDataset.GetSingleValueOrDefault<DateTime>(dicomTag, expectedVR: expectedVR).Ticks;
        }
        catch (Exception)
        {
            result = null;
        }

        // If parsing fails for Time, a default value of 0 is returned. Since expected outcome is null, testing here and returning expected result.
        return result == 0 ? null : result;
    }

    /// <summary>
    /// Creates a new copy of DICOM dataset with items of VR types considered to be bulk data removed.
    /// </summary>
    /// <param name="dicomDataset">The DICOM dataset.</param>
    /// <returns>A copy of the <paramref name="dicomDataset"/> with items of VR types considered  to be bulk data removed.</returns>
    public static DicomDataset CopyWithoutBulkDataItems(this DicomDataset dicomDataset)
    {
        EnsureArg.IsNotNull(dicomDataset, nameof(dicomDataset));

        return CopyDicomDatasetWithoutBulkDataItems(dicomDataset);

        static DicomDataset CopyDicomDatasetWithoutBulkDataItems(DicomDataset dicomDatasetToCopy)
        {
            return new DicomDataset(dicomDatasetToCopy
                .Select(dicomItem =>
                {
                    if (DicomBulkDataVr.Contains(dicomItem.ValueRepresentation))
                    {
                        // If the VR is bulk data type, return null so it can be filtered out later.
                        return null;
                    }
                    else if (dicomItem.ValueRepresentation == DicomVR.SQ)
                    {
                        // If the VR is sequence, then process each item within the sequence.
                        DicomSequence sequenceToCopy = (DicomSequence)dicomItem;

                        return new DicomSequence(
                            sequenceToCopy.Tag,
                            sequenceToCopy.Select(itemToCopy => itemToCopy.CopyWithoutBulkDataItems()).ToArray());
                    }
                    else
                    {
                        // The VR is not bulk data, return it.
                        return dicomItem;
                    }
                })
                .Where(dicomItem => dicomItem != null));
        }
    }

    /// <summary>
    /// Creates an instance of <see cref="InstanceIdentifier"/> from <see cref="DicomDataset"/>.
    /// </summary>
    /// <param name="dicomDataset">The DICOM dataset to get the identifiers from.</param>
    /// <param name="partitionKey">Data Partition key</param>
    /// <returns>An instance of <see cref="InstanceIdentifier"/> representing the <paramref name="dicomDataset"/>.</returns>
    public static InstanceIdentifier ToInstanceIdentifier(this DicomDataset dicomDataset, int partitionKey = default)
    {
        EnsureArg.IsNotNull(dicomDataset, nameof(dicomDataset));

        // Note: Here we 'GetSingleValueOrDefault' and let the constructor validate the identifier.
        return new InstanceIdentifier(
            dicomDataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty),
            dicomDataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty),
            dicomDataset.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, string.Empty),
            partitionKey);
    }

    /// <summary>
    /// Creates an instance of <see cref="VersionedInstanceIdentifier"/> from <see cref="DicomDataset"/>.
    /// </summary>
    /// <param name="dicomDataset">The DICOM dataset to get the identifiers from.</param>
    /// <param name="version">The version.</param>
    /// <param name="partitionKey">Data Partition key</param>
    /// <returns>An instance of <see cref="InstanceIdentifier"/> representing the <paramref name="dicomDataset"/>.</returns>
    public static VersionedInstanceIdentifier ToVersionedInstanceIdentifier(this DicomDataset dicomDataset, long version, int partitionKey = default)
    {
        EnsureArg.IsNotNull(dicomDataset, nameof(dicomDataset));

        // Note: Here we 'GetSingleValueOrDefault' and let the constructor validate the identifier.
        return new VersionedInstanceIdentifier(
            dicomDataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty),
            dicomDataset.GetSingleValueOrDefault(DicomTag.SeriesInstanceUID, string.Empty),
            dicomDataset.GetSingleValueOrDefault(DicomTag.SOPInstanceUID, string.Empty),
            version,
            partitionKey);
    }

    /// <summary>
    /// Adds value to the <paramref name="dicomDataset"/> if <paramref name="value"/> is not null.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="dicomDataset">The dataset to add value to.</param>
    /// <param name="dicomTag">The DICOM tag.</param>
    /// <param name="value">The value to add.</param>
    public static void AddValueIfNotNull<T>(this DicomDataset dicomDataset, DicomTag dicomTag, T value)
        where T : class
    {
        EnsureArg.IsNotNull(dicomDataset, nameof(dicomDataset));
        EnsureArg.IsNotNull(dicomTag, nameof(dicomTag));

        if (value != null)
        {
            dicomDataset.Add(dicomTag, value);
        }
    }

    /// <summary>
    /// Validate query tag in Dicom dataset.
    /// </summary>
    /// <param name="dataset">The dicom dataset.</param>
    /// <param name="queryTag">The query tag.</param>
    /// <param name="minimumValidator">The minimum validator.</param>
    public static void ValidateQueryTag(this DicomDataset dataset, QueryTag queryTag, IElementMinimumValidator minimumValidator)
    {
        EnsureArg.IsNotNull(dataset, nameof(dataset));
        EnsureArg.IsNotNull(queryTag, nameof(queryTag));
        EnsureArg.IsNotNull(minimumValidator, nameof(minimumValidator));
        DicomElement dicomElement = dataset.GetDicomItem<DicomElement>(queryTag.Tag);

        if (dicomElement != null)
        {
            if (dicomElement.ValueRepresentation != queryTag.VR)
            {
                string name = dicomElement.Tag.GetFriendlyName();
                DicomVR actualVR = dicomElement.ValueRepresentation;
                throw new ElementValidationException(
                    name,
                    actualVR,
                    ValidationErrorCode.UnexpectedVR,
                    string.Format(CultureInfo.InvariantCulture, DicomCoreResource.ErrorMessageUnexpectedVR, name, queryTag.VR, actualVR));
            }

            minimumValidator.Validate(dicomElement);
        }
    }

    /// <summary>
    /// Gets DicomDatasets that matches a list of tags reprenting a tag path.
    /// </summary>
    /// <param name="dataset">The DicomDataset to be traversed.</param>
    /// <param name="dicomTags">The Dicom tags modelling the path.</param>
    /// <returns>Lists of DicomDataset that matches the list of tags.</returns>
    public static IEnumerable<DicomDataset> GetSequencePathValues(this DicomDataset dataset, ReadOnlyCollection<DicomTag> dicomTags)
    {
        EnsureArg.IsNotNull(dataset, nameof(dataset));
        EnsureArg.IsNotNull(dicomTags, nameof(dicomTags));

        if (dicomTags.Count != 2)
        {
            throw new ElementValidationException(string.Join(", ", dicomTags.Select(x => x.GetPath())), DicomVR.SQ, ValidationErrorCode.NestedSequence);
        }

        var foundSequence = dataset.GetSequence(dicomTags[0]);
        if (foundSequence != null)
        {
            foreach (var childDataset in foundSequence.Items)
            {
                var item = childDataset.GetDicomItem<DicomItem>(dicomTags[1]);

                if (item != null)
                {
                    yield return new DicomDataset(item);
                }
            }
        }
    }

    /// <summary>
    /// Validate whether a dataset meets the service class user (SCU) and service class provider (SCP) requirements for a given attribute.
    /// <see href="https://dicom.nema.org/medical/dicom/current/output/html/part04.html#sect_5.4.2.1">Dicom 3.4.5.4.2.1</see>
    /// </summary>
    /// <param name="dataset">The dataset to validate.</param>
    /// <param name="tag">The tag for the attribute that is required.</param>
    /// <param name="requirement">The requirement code expressed as an enum.</param>
    public static void ValidateRequirement(this DicomDataset dataset, DicomTag tag, RequirementCode requirement)
    {
        EnsureArg.IsNotNull(dataset, nameof(dataset));
        EnsureArg.IsNotNull(tag, nameof(tag));

        // We only validate attributes that are mandatory for the SCU (1 or 2). We won't validate any optional attributes (3).
        dataset.ValidateRequiredAttribute(tag, (requirement is RequirementCode.OneOne));
    }

    public static void ValidateRequirement(
        this DicomDataset dataset,
        DicomTag tag,
        ProcedureStepState targetProcedureStepState,
        FinalStateRequirementCode requirement,
        Func<DicomDataset, DicomTag, bool> requirementCondition = default)
    {
        EnsureArg.IsNotNull(dataset, nameof(dataset));
        EnsureArg.IsNotNull(tag, nameof(tag));

        var predicate = (requirementCondition == default) ? (ds, tag) => false : requirementCondition;

        if (targetProcedureStepState != ProcedureStepState.Completed &&
            targetProcedureStepState != ProcedureStepState.Canceled)
        {
            return;
        }

        switch (requirement)
        {
            case FinalStateRequirementCode.R:
                dataset.ValidateRequiredAttribute(tag);
                break;
            case FinalStateRequirementCode.RC:
                if (predicate(dataset, tag))
                {
                    dataset.ValidateRequiredAttribute(tag);
                }

                break;
            case FinalStateRequirementCode.P:
                if (ProcedureStepState.Completed == targetProcedureStepState)
                {
                    dataset.ValidateRequiredAttribute(tag);
                }

                break;
            case FinalStateRequirementCode.X:
                if (ProcedureStepState.Canceled == targetProcedureStepState)
                {
                    dataset.ValidateRequiredAttribute(tag);
                }

                break;
            case FinalStateRequirementCode.O:
                break;
        }
    }

    private static void ValidateRequiredAttribute(this DicomDataset dataset, DicomTag tag, bool valueCannotBeZeroLength = true)
    {
        EnsureArg.IsNotNull(dataset, nameof(dataset));

        if (!dataset.Contains(tag))
        {
            throw new DatasetValidationException(
                FailureReasonCodes.MissingAttribute,
                string.Format(
                    CultureInfo.InvariantCulture,
                    DicomCoreResource.MissingRequiredTag,
                    tag));
        }

        if (valueCannotBeZeroLength && dataset.GetValueCount(tag) < 1)
        {
            throw new DatasetValidationException(
                FailureReasonCodes.MissingAttributeValue,
                string.Format(
                    CultureInfo.InvariantCulture,
                    DicomCoreResource.MissingRequiredValue,
                    tag));
        }
    }
}
