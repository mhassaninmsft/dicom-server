// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------



using Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;

namespace Microsoft.Health.Dicom.CosmosDb
{
    public partial class CosmosDataStore : IExtendedQueryTagStore
    {
        public Task<IReadOnlyList<ExtendedQueryTagStoreEntry>> AddExtendedQueryTagsAsync(IReadOnlyCollection<AddExtendedQueryTagEntry> extendedQueryTagEntries, int maxAllowedCount, bool ready = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ExtendedQueryTagStoreEntry>> AssignReindexingOperationAsync(IReadOnlyCollection<int> queryTagKeys, Guid operationId, bool returnIfCompleted = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<int>> CompleteReindexingAsync(IReadOnlyCollection<int> queryTagKeys, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteExtendedQueryTagAsync(string tagPath, string vr, CancellationToken cancellationToken = default)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public Task<ExtendedQueryTagStoreJoinEntry> GetExtendedQueryTagAsync(string tagPath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ExtendedQueryTagStoreJoinEntry>> GetExtendedQueryTagsAsync(int limit, int offset = 0, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new List<ExtendedQueryTagStoreJoinEntry>() as IReadOnlyList<ExtendedQueryTagStoreJoinEntry>);

        }

        public Task<IReadOnlyList<ExtendedQueryTagStoreJoinEntry>> GetExtendedQueryTagsAsync(IReadOnlyCollection<int> queryTagKeys, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new List<ExtendedQueryTagStoreJoinEntry>() as IReadOnlyList<ExtendedQueryTagStoreJoinEntry>);
        }

        public Task<IReadOnlyList<ExtendedQueryTagStoreEntry>> GetExtendedQueryTagsAsync(Guid operationId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new List<ExtendedQueryTagStoreEntry>() as IReadOnlyList<ExtendedQueryTagStoreEntry>);
        }

        public Task<ExtendedQueryTagStoreJoinEntry> UpdateQueryStatusAsync(string tagPath, QueryStatus queryStatus, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
            //var res =new  ExtendedQueryTagStoreJoinEntry()
            //return Task.FromResult(new List<ExtendedQueryTagStoreJoinEntry>() as IReadOnlyList<ExtendedQueryTagStoreJoinEntry>);

        }
    }
}
