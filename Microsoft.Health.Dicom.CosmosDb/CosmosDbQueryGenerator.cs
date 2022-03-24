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
using Microsoft.Health.Dicom.Core.Features.Query;
using System.Text.RegularExpressions;

namespace Microsoft.Health.Dicom.CosmosDb
{
    public class CosmosDbQueryGenerator : QueryFilterConditionVisitor
    {
        private string _query = "";
        public override void Visit(StringSingleValueMatchCondition stringSingleValueMatchCondition)
        {
            EnsureArg.IsNotNull(stringSingleValueMatchCondition, nameof(stringSingleValueMatchCondition));
            string tagName = stringSingleValueMatchCondition.QueryTag.GetName();
            string tagValue = stringSingleValueMatchCondition.Value;
            string condition = $"(c['{tagName}'] = {tagValue})";
            AddToQuery(condition);

        }
        private void AddToQuery(string val)
        {
            if (_query.Length == 0)
            {

            }
            else
            {
                _query += "AND"; // *** TODO *** does this need spaces?
            }
            _query += val;
        }

        public string OutputQuery()
        {
            return _query;
        }

        public override void Visit(DateRangeValueMatchCondition rangeValueMatchCondition)
        {
            // Ensure not null
            EnsureArg.IsNotNull(rangeValueMatchCondition, nameof(rangeValueMatchCondition));
            //already has tag and value


            // ****** DONE *** do we need the equivalent of the DicomTageSqlEntry.cs? YES

            // *** TODO *** Do we need the equivalents of:
            // //using Microsoft.Health.Dicom.SqlServer.Features.Schema.Model;
            // //using Microsoft.Health.SqlServer.Features.Schema.Model;

            // ****** DONE *** do we need equivalent of GetTableAlias? YES-ish/maybe

            //get the tag
            // *** TODO *** limit the date comparisons to the two tags supported currently

            var queryTag = rangeValueMatchCondition.QueryTag;
            var tagName = queryTag.Tag.ToString().Trim('(', ')');
            tagName = Regex.Replace(tagName, ",", "");
            var fromDate = rangeValueMatchCondition.Minimum;
            var fromDateFormatted = fromDate.ToString("yyyyMMdd");
            var toDate = rangeValueMatchCondition.Maximum;
            var toDateFormatted = toDate.ToString("yyyyMMdd");

            // use BETWEEN-equivalent
            // INCLUSIVE
            // ***TODO** ideally not string plaintext to prevent sql injection
            // c["value"]["00080020"]["Value"][0] <= "20220324"
            string condition = $"(c['value']['{tagName}']['Value'][0] BETWEEN '{fromDateFormatted}' AND '{toDateFormatted}')";

            //INSPO: (later)
            //****** DONE *** do we want Linq? eventually
            //IQueryable<Order> orders = container.GetItemLinqQueryable<Order>(allowSynchronousQueryExecution: true).Where(o => o.ShipDate >= DateTime.UtcNow.AddDays(-3));

            AddToQuery(condition);
            //throw new NotImplementedException();
        }

        public override void Visit(DateSingleValueMatchCondition dateSingleValueMatchCondition)
        {
            EnsureArg.IsNotNull(dateSingleValueMatchCondition, nameof(dateSingleValueMatchCondition));
            string tagName = dateSingleValueMatchCondition.QueryTag.GetName();
            string tagValue = dateSingleValueMatchCondition.Value.ToString();
            string condition = $"(c['{tagName}'] = {tagValue})";
            AddToQuery(condition);
        }

        public override void Visit(PersonNameFuzzyMatchCondition fuzzyMatchCondition)
        {
            throw new NotImplementedException();
        }

        public override void Visit(DoubleSingleValueMatchCondition doubleSingleValueMatchCondition)
        {
            throw new NotImplementedException();
        }

        public override void Visit(LongRangeValueMatchCondition longRangeValueMatchCondition)
        {
            throw new NotImplementedException();
        }

        public override void Visit(LongSingleValueMatchCondition longSingleValueMatchCondition)
        {
            throw new NotImplementedException();
        }
    }
}
