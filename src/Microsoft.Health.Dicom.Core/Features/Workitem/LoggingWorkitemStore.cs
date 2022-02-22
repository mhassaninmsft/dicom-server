﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using FellowOakDicom;
using Microsoft.Extensions.Logging;

namespace Microsoft.Health.Dicom.Core.Features.Workitem
{
    public class LoggingWorkitemStore : IWorkitemStore
    {
        private static readonly Action<ILogger, WorkitemInstanceIdentifier, Exception> LogAddWorkitemDelegate =
               LoggerMessage.Define<WorkitemInstanceIdentifier>(
                   LogLevel.Debug,
                   default,
                   "Adding workitem '{WorkitemInstanceIdentifier}'.");

        private static readonly Action<ILogger, WorkitemInstanceIdentifier, Exception> LogQueryWorkitemDelegate =
               LoggerMessage.Define<WorkitemInstanceIdentifier>(
                   LogLevel.Debug,
                   default,
                   "Querying workitem '{WorkitemInstanceIdentifier}'.");

        private static readonly Action<ILogger, Exception> LogOperationSucceededDelegate =
            LoggerMessage.Define(
                LogLevel.Debug,
                default,
                "The operation completed successfully.");

        private static readonly Action<ILogger, Exception> LogOperationFailedDelegate =
            LoggerMessage.Define(
                LogLevel.Warning,
                default,
                "The operation failed.");

        private readonly IWorkitemStore _workitemStore;
        private readonly ILogger _logger;

        public LoggingWorkitemStore(IWorkitemStore workitemStore, ILogger<LoggingWorkitemStore> logger)
        {
            EnsureArg.IsNotNull(workitemStore, nameof(workitemStore));
            EnsureArg.IsNotNull(logger, nameof(logger));

            _workitemStore = workitemStore;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task AddWorkitemAsync(WorkitemInstanceIdentifier identifier, DicomDataset dataset, CancellationToken cancellationToken)
        {
            EnsureArg.IsNotNull(identifier, nameof(identifier));

            LogAddWorkitemDelegate(_logger, identifier, null);

            try
            {
                await _workitemStore.AddWorkitemAsync(identifier, dataset, cancellationToken);

                LogOperationSucceededDelegate(_logger, null);
            }
            catch (Exception ex)
            {
                LogOperationFailedDelegate(_logger, ex);

                throw;
            }
        }

        public async Task<DicomDataset> GetWorkitemAsync(WorkitemInstanceIdentifier workitemInstanceIdentifier, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNull(workitemInstanceIdentifier, nameof(workitemInstanceIdentifier));

            LogQueryWorkitemDelegate(_logger, workitemInstanceIdentifier, null);

            try
            {
                var result = await _workitemStore.GetWorkitemAsync(workitemInstanceIdentifier, cancellationToken);

                LogOperationSucceededDelegate(_logger, null);

                return result;
            }
            catch (Exception ex)
            {
                LogOperationFailedDelegate(_logger, ex);

                throw;
            }
        }
    }
}
