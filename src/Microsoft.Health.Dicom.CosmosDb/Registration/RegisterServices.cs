// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Dicom.Core.Features.Common;
using Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;
using Microsoft.Health.Dicom.Core.Features.Query;
using Microsoft.Health.Dicom.Core.Features.Retrieve;
using Microsoft.Health.Dicom.Core.Features.Store;
using Microsoft.Health.Dicom.Core.Registration;
using Microsoft.Health.Dicom.CosmosDb.Config;

namespace Microsoft.Health.Dicom.CosmosDb.Registration
{
    public static class RegisterServices
    {
        public static IDicomServerBuilder AddCosmosDB(this IDicomServerBuilder serverBuilder, IConfiguration configuration)
        {
            EnsureArg.IsNotNull(serverBuilder, nameof(serverBuilder));
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            serverBuilder.Services.AddOptions<CosmosDbConfig>()
          .Bind(configuration.GetSection("CosmosDataStore"), binderOptions =>
          {
              binderOptions.ErrorOnUnknownConfiguration = true;
          });
            serverBuilder.Services.AddSingleton<IMetadataStore, CosmosDataStore>();
            serverBuilder.Services.AddSingleton<IIndexDataStore, CosmosDataStore>();
            serverBuilder.Services.AddSingleton<IInstanceStore, CosmosDataStore>();
            serverBuilder.Services.AddSingleton<IQueryStore, CosmosDataStore>();
            serverBuilder.Services.AddScoped<IExtendedQueryTagStore, CosmosDataStore>();
            return serverBuilder;

        }
    }
}
