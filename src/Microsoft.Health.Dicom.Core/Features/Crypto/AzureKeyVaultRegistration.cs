// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Health.Dicom.Core.Registration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Core;
using Azure.Identity;

namespace Microsoft.Health.Dicom.Core.Features.Crypto;
public static class AzureKeyVaultRegistration
{
    public static IDicomServerBuilder AddAzureKeyVault(this IDicomServerBuilder serverBuilder, IConfiguration configuration)
    {
        EnsureArg.IsNotNull(serverBuilder, nameof(serverBuilder));
        EnsureArg.IsNotNull(configuration, nameof(configuration));

        serverBuilder.Services.AddOptions<AzureKeyVaultConfig>().
            Bind(configuration.GetSection("AzureKeyVault"), binderOptions =>
            {
                binderOptions.ErrorOnUnknownConfiguration = true;
            });
        // If we should use a service principal vs a managed identity
        var useServicePrincipal = true;
        if (useServicePrincipal)
        {
            // serverBuilder.Services.AddOptions<AzureKeyVaultConfig>().
            //Bind(configuration.GetSection("AzureKeyVault"), binderOptions =>
            //{
            //    binderOptions.ErrorOnUnknownConfiguration = true;
            //});
            var azspcreds = new AzureServicePrincipalCredentials();
            configuration.GetSection("AzureServicePrincipalCredentials").Bind(azspcreds);
            serverBuilder.Services.AddSingleton<TokenCredential>(new ClientSecretCredential(azspcreds.TenantId, azspcreds.ClientId, azspcreds.ClientSecret));

        }
        else
        {
            //use the Managed Identity default implemnation (must be running in an Enviornemnt that has permission to talk to the Keyvault
            serverBuilder.Services.AddSingleton<TokenCredential>(new DefaultAzureCredential());
        }
        return serverBuilder;

    }
}
