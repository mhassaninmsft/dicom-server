﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Data.SqlClient;
using Microsoft.Health.Dicom.Core.Features.ChangeFeed;
using Microsoft.Health.Dicom.SqlServer.Features.Schema;
using Microsoft.Health.Dicom.SqlServer.Features.Schema.Model;
using Microsoft.Health.SqlServer.Features.Client;
using Microsoft.Health.SqlServer.Features.Storage;

namespace Microsoft.Health.Dicom.SqlServer.Features.ChangeFeed
{
    public class SqlChangeFeedStoreV1 : ISqlChangeFeedStore
    {
        public virtual SchemaVersion Version => SchemaVersion.V1;

        protected SqlConnectionWrapperFactory SqlConnectionWrapperFactory { get; }

        public SqlChangeFeedStoreV1(SqlConnectionWrapperFactory sqlConnectionWrapperFactory)
        {
            SqlConnectionWrapperFactory = EnsureArg.IsNotNull(sqlConnectionWrapperFactory, nameof(sqlConnectionWrapperFactory));
        }

        public virtual async Task<ChangeFeedEntry> GetChangeFeedLatestAsync(CancellationToken cancellationToken)
        {
            using SqlConnectionWrapper sqlConnectionWrapper = await SqlConnectionWrapperFactory.ObtainSqlConnectionWrapperAsync(cancellationToken);
            using SqlCommandWrapper sqlCommandWrapper = sqlConnectionWrapper.CreateSqlCommand();

            V4.GetChangeFeedLatest.PopulateCommand(sqlCommandWrapper);

            using SqlDataReader reader = await sqlCommandWrapper.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                (long rSeq, DateTimeOffset rTimestamp, int rAction, string rStudyInstanceUid, string rSeriesInstanceUid, string rSopInstanceUid, long oWatermark, long? cWatermark) = reader.ReadRow(
                    V4.ChangeFeed.Sequence,
                    V4.ChangeFeed.Timestamp,
                    V4.ChangeFeed.Action,
                    V4.ChangeFeed.StudyInstanceUid,
                    V4.ChangeFeed.SeriesInstanceUid,
                    V4.ChangeFeed.SopInstanceUid,
                    V4.ChangeFeed.OriginalWatermark,
                    V4.ChangeFeed.CurrentWatermark);

                return new ChangeFeedEntry(
                        rSeq,
                        rTimestamp,
                        (ChangeFeedAction)rAction,
                        rStudyInstanceUid,
                        rSeriesInstanceUid,
                        rSopInstanceUid,
                        oWatermark,
                        cWatermark,
                        ConvertWatermarkToCurrentState(oWatermark, cWatermark));
            }

            return null;
        }

        public virtual async Task<IReadOnlyCollection<ChangeFeedEntry>> GetChangeFeedAsync(long offset, int limit, CancellationToken cancellationToken)
        {
            var results = new List<ChangeFeedEntry>();

            using SqlConnectionWrapper sqlConnectionWrapper = await SqlConnectionWrapperFactory.ObtainSqlConnectionWrapperAsync(cancellationToken);
            using SqlCommandWrapper sqlCommandWrapper = sqlConnectionWrapper.CreateSqlCommand();

            V4.GetChangeFeed.PopulateCommand(sqlCommandWrapper, limit, offset);

            using SqlDataReader reader = await sqlCommandWrapper.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                (long rSeq, DateTimeOffset rTimestamp, int rAction, string rStudyInstanceUid, string rSeriesInstanceUid, string rSopInstanceUid, long oWatermark, long? cWatermark) = reader.ReadRow(
                    V4.ChangeFeed.Sequence,
                    V4.ChangeFeed.Timestamp,
                    V4.ChangeFeed.Action,
                    V4.ChangeFeed.StudyInstanceUid,
                    V4.ChangeFeed.SeriesInstanceUid,
                    V4.ChangeFeed.SopInstanceUid,
                    V4.ChangeFeed.OriginalWatermark,
                    V4.ChangeFeed.CurrentWatermark);

                results.Add(new ChangeFeedEntry(
                        rSeq,
                        rTimestamp,
                        (ChangeFeedAction)rAction,
                        rStudyInstanceUid,
                        rSeriesInstanceUid,
                        rSopInstanceUid,
                        oWatermark,
                        cWatermark,
                        ConvertWatermarkToCurrentState(oWatermark, cWatermark)));
            }

            return results;
        }

        protected static ChangeFeedState ConvertWatermarkToCurrentState(long originalWatermark, long? currentWatermak)
        {
            if (currentWatermak == null)
            {
                return ChangeFeedState.Deleted;
            }

            if (currentWatermak != originalWatermark)
            {
                return ChangeFeedState.Replaced;
            }

            return ChangeFeedState.Current;
        }
    }
}