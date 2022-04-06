// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace Microsoft.Health.Dicom.CosmosDb.Config
{
    public record CosmosDbConfig
    {
        [Required()]
        // The Azure Cosmos DB endpoint for running this sample.
        public Uri EndpointUri { get; init; } = new Uri("https://dummy.com");
        [Required()]
        // The primary key for the Azure Cosmos account.
        public string PrimaryKey { get; init; } = string.Empty;
        [Required()]
        // The name of the database and container we will create
        public string DatabaseId { get; init; } = string.Empty;
        [Required()]
        public string ContainerId { get; init; } = string.Empty;
    }
}
