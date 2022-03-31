﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Microsoft.Health.Dicom.Core.Features.Query.Model.FilterConditions;

namespace Microsoft.Health.Dicom.Core.Features.Query.Model;

/// <summary>
/// Valid parsed object representing query parameters for a QIDO-RS request
/// </summary>
public class BaseQueryExpression
{
    public BaseQueryExpression(
        QueryIncludeField includeFields,
        bool fuzzyMatching,
        int limit,
        int offset,
        IReadOnlyCollection<QueryFilterCondition> filterConditions)
    {
        IncludeFields = includeFields;
        FuzzyMatching = fuzzyMatching;
        Limit = limit;
        Offset = offset;
        FilterConditions = EnsureArg.IsNotNull(filterConditions, nameof(filterConditions));
    }

    /// <summary>
    /// Dicom tags to include in query result
    /// </summary>
    public QueryIncludeField IncludeFields { get; }

    /// <summary>
    /// If true do Fuzzy matching of PN tag types
    /// </summary>
    public bool FuzzyMatching { get; }

    /// <summary>
    /// Query result count
    /// </summary>
    public int Limit { get; }

    /// <summary>
    /// Query result skip offset count
    /// </summary>
    public int Offset { get; }

    /// <summary>
    /// List of filter conditions to find the DICOM objects
    /// </summary>
    public IReadOnlyCollection<QueryFilterCondition> FilterConditions { get; }

    /// <summary>
    /// Request query was empty
    /// </summary>
    public bool HasFilters
    {
        get
        {
            return FilterConditions.Any();
        }
    }

    /// <summary>
    /// evaluted result count for this request
    /// </summary>
    public int EvaluatedLimit
    {
        get
        {
            return Limit > 0 && Limit <= QueryLimit.MaxQueryResultCount ?
                Limit : QueryLimit.DefaultQueryResultCount;
        }
    }
}
