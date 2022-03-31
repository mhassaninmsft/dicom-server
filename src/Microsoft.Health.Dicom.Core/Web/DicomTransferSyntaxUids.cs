﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using FellowOakDicom;

namespace Microsoft.Health.Dicom.Core.Web;

public static class DicomTransferSyntaxUids
{
    public const string Original = "*";
    public const string ExplicitVRLittleEndian = "1.2.840.10008.1.2.1";

    public static bool IsOriginalTransferSyntaxRequested(string transferSyntax)
    {
        return Original.Equals(transferSyntax, StringComparison.Ordinal);
    }

    public static bool AreEqual(string transferSyntaxA, string transferSyntaxB)
    {
        EnsureArg.IsNotNull(transferSyntaxA, nameof(transferSyntaxA));
        EnsureArg.IsNotNull(transferSyntaxB, nameof(transferSyntaxB));

        return DicomTransferSyntax.Parse(transferSyntaxA) == DicomTransferSyntax.Parse(transferSyntaxB);
    }
}
