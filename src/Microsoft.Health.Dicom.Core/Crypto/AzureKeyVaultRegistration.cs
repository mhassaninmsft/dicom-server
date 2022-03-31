// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------



using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Health.Dicom.Core.Registration;
using Microsoft.Extensions.DependencyInjection;
namespace Microsoft.Health.Dicom.Core.Crypto;
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
            serverBuilder.Services.AddOptions<AzureKeyVaultConfig>().
           Bind(configuration.GetSection("AzureKeyVault"), binderOptions =>
           {
               binderOptions.ErrorOnUnknownConfiguration = true;
           });
        }
        else
        {

        }
        return serverBuilder;

    }
}
