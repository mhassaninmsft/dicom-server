// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

//using System;
using System.Collections.Generic;
using System.Linq;
using EnsureThat;
using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Primitives;
using Microsoft.Health.Dicom.Core.Messages.Retrieve;
using Microsoft.Net.Http.Headers;

namespace Microsoft.Health.Dicom.Api.Extensions
{
    public static class HttpRequestExtensions
    {
        public static IEnumerable<AcceptHeader> GetAcceptHeaders(this HttpRequest httpRequest)
        {
            EnsureArg.IsNotNull(httpRequest, nameof(httpRequest));
            var headers = httpRequest.GetTypedHeaders();
            var rte = headers.Accept;
            //IList<MediaTypeHeaderValue> acceptHeaders = headers.Accept;
            //IList<MediaTypeHeaderValue> acceptHeaders = httpRequest.GetTypedHeaders().Accept;
            var res1 = httpRequest.Headers.GetCommaSeparatedValues(HeaderNames.Accept);
            //IList<MediaTypeHeaderValue> acceptHeaders = res1.Select(x => new MediaTypeHeaderValue(new StringSegment(x, 0, x.Length))).ToList();
            IList<MediaTypeHeaderValue> acceptHeaders = headers.Accept;
            //StringSegment seg1 = new StringSegment();
            //MediaTypeHeaderValue val1 = new MediaTypeHeaderValue(seg1);
            //Console.WriteLine($"")
            if (acceptHeaders != null && acceptHeaders.Count != 0)
            {
                return acceptHeaders.Select((item) => item.ToAcceptHeader())
                    .ToList();
            }

            return Enumerable.Empty<AcceptHeader>();
        }
    }
}
