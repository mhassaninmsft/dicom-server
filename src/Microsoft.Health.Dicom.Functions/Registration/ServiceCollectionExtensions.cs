﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Health.Blob.Configs;
using Microsoft.Health.Dicom.Functions.Configuration;
using Microsoft.Health.Dicom.Functions.Durable;
using Microsoft.Health.Dicom.Functions.Registration;
using Microsoft.Health.SqlServer.Configs;
using Microsoft.IO;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFunctionsOptions<T>(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName)
            where T : class
        {
            EnsureArg.IsNotNull(services, nameof(services));
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            EnsureArg.IsNotEmptyOrWhiteSpace(sectionName, nameof(sectionName));

            services.Configure<T>(configuration
                .GetSection(DicomFunctionsConfiguration.SectionName)
                .GetSection(sectionName));

            return services;
        }

        public static IServiceCollection AddDurableFunctionServices(this IServiceCollection services)
        {
            EnsureArg.IsNotNull(services, nameof(services));

            services.TryAddSingleton(GuidFactory.Default);

            return services;
        }

        public static IServiceCollection AddRecyclableMemoryStreamManager(this IServiceCollection services, Func<RecyclableMemoryStreamManager> factory = null)
        {
            EnsureArg.IsNotNull(services, nameof(services));

            // The custom service provider used by Azure Functions cannot seem to resolve the
            // RecyclableMemoryStreamManager ctor overloads without help, so we instantiate it ourselves
            factory ??= () => new RecyclableMemoryStreamManager();
            services.TryAddSingleton(factory());

            return services;
        }

        public static IServiceCollection AddStorageServices(this IServiceCollection services, IConfiguration configuration)
        {
            EnsureArg.IsNotNull(services, nameof(services));
            EnsureArg.IsNotNull(configuration, nameof(configuration));

            IConfigurationSection blobSection = configuration.GetSection(BlobDataStoreConfiguration.SectionName);
            new DicomFunctionsBuilder(services)
                .AddSqlServer(c => configuration.GetSection(SqlServerDataStoreConfiguration.SectionName).Bind(c))
                .AddMetadataStorageDataStore(
                    blobSection.GetSection(DicomBlobContainerConfiguration.SectionName).Get<DicomBlobContainerConfiguration>().Metadata,
                    c => blobSection.Bind(c));

            return services;
        }

        public static IServiceCollection AddHttpServices(this IServiceCollection services)
        {
            EnsureArg.IsNotNull(services, nameof(services));

            services
                .AddControllers()
                .AddJsonSerializerOptions(x => x.Converters.Add(new JsonStringEnumConverter()));

            return services;
        }

        public static IMvcBuilder AddJsonSerializerOptions(this IMvcBuilder builder, Action<JsonSerializerOptions> configure)
        {
            EnsureArg.IsNotNull(builder, nameof(builder));
            EnsureArg.IsNotNull(configure, nameof(configure));

            // TODO: Configure System.Text.Json for Azure Functions when available
            //builder.AddJsonOptions(o => configure(o.JsonSerializerOptions));
            builder.Services.Configure(configure);
            return builder;
        }
    }
}