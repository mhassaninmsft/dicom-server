﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Health.Dicom.Core.Features.Common;
using Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;
using Microsoft.Health.Dicom.Features.Common;

namespace Microsoft.Health.Dicom.Core.Features.Workitem
{
    public class WorkitemQueryTagService : IWorkitemQueryTagService, IDisposable
    {
        private readonly IIndexWorkitemStore _indexWorkitemStore;
        private readonly AsyncCache<IReadOnlyCollection<QueryTag>> _queryTagCache;
        private readonly IDicomTagParser _dicomTagParser;
        private bool _disposed;

        public WorkitemQueryTagService(IIndexWorkitemStore indexWorkitemStore, IDicomTagParser dicomTagParser)
        {
            _indexWorkitemStore = EnsureArg.IsNotNull(indexWorkitemStore, nameof(indexWorkitemStore));
            _queryTagCache = new AsyncCache<IReadOnlyCollection<QueryTag>>(ResolveQueryTagsAsync);
            _dicomTagParser = EnsureArg.IsNotNull(dicomTagParser, nameof(dicomTagParser));
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _queryTagCache.Dispose();
                }

                _disposed = true;
            }
        }

        public async Task<IReadOnlyCollection<QueryTag>> GetQueryTagsAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(WorkitemQueryTagService));
            }

            return await _queryTagCache.GetAsync(cancellationToken: cancellationToken);
        }

        private async Task<IReadOnlyCollection<QueryTag>> ResolveQueryTagsAsync(CancellationToken cancellationToken)
        {
            var workitemQueryTags = await _indexWorkitemStore.GetWorkitemQueryTagsAsync(cancellationToken);

            foreach (var tag in workitemQueryTags)
            {
                if (_dicomTagParser.TryParseToDicomItem(tag.Path, out var dicomItem))
                {
                    tag.Item = dicomItem;
                }
            }

            return workitemQueryTags.Select(x => new QueryTag(x)).ToList();
        }
    }
}