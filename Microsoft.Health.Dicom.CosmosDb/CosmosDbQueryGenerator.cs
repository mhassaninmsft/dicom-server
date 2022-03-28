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
                _query += " AND "; // *** TODO *** does this need spaces?
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

            //get the tag
            // *** TODO *** limit the date comparisons to the two tags supported currently

            var queryTag = rangeValueMatchCondition.QueryTag; // initally formatted as "(####,####)"
            var tagName = queryTag.Tag.ToString().Trim('(', ')');
            tagName = Regex.Replace(tagName, ",", "");

            var fromDate = rangeValueMatchCondition.Minimum;
            var toDate = rangeValueMatchCondition.Maximum;
            // match on formatting written to cosmos
            var fromDateFormatted = fromDate.ToString("yyyyMMdd");
            var toDateFormatted = toDate.ToString("yyyyMMdd");

            // ***TODO** ideally not string plaintext to prevent sql injection
            string condition = $"(c['value']['{tagName}']['Value'][0] BETWEEN '{fromDateFormatted}' AND '{toDateFormatted}')";

            //INSPO: (later)
            //****** DONE *** do we want Linq? eventually
            //IQueryable<Order> orders = container.GetItemLinqQueryable<Order>(allowSynchronousQueryExecution: true).Where(o => o.ShipDate >= DateTime.UtcNow.AddDays(-3));

            AddToQuery(condition);
        }

        public override void Visit(DateSingleValueMatchCondition dateSingleValueMatchCondition)
        {
            EnsureArg.IsNotNull(dateSingleValueMatchCondition, nameof(dateSingleValueMatchCondition));
            var queryTag = dateSingleValueMatchCondition.QueryTag;
            var tagName = queryTag.Tag.ToString().Trim('(', ')');
            tagName = Regex.Replace(tagName, ",", "");
            string tagValue = dateSingleValueMatchCondition.Value.ToString("yyyyMMdd");
            string condition = $"(c['value']['{tagName}']['Value'][0] = {tagValue})";
            AddToQuery(condition);
        }

        public override void Visit(PersonNameFuzzyMatchCondition fuzzyMatchCondition)
        {
            EnsureArg.IsNotNull(fuzzyMatchCondition, nameof(fuzzyMatchCondition));
            var queryTag = fuzzyMatchCondition.QueryTag;
            var tagName = queryTag.Tag.ToString().Trim('(', ')');
            tagName = Regex.Replace(tagName, ",", "");
            var tagValue = fuzzyMatchCondition.Value.ToString();
            var condition = $"STARTSWITH(c['value']['{tagName}']['Value'][0]['Alphabetic'], '{tagValue}')";
            //throw new NotImplementedException();
            AddToQuery(condition);
        }

        public override void Visit(DoubleSingleValueMatchCondition doubleSingleValueMatchCondition)
        {
            //Ensure the condition is not null
            EnsureArg.IsNotNull(doubleSingleValueMatchCondition, nameof(doubleSingleValueMatchCondition));
            //doubleSingleValueMatchCondition;
            // get the tag & value
            var queryTag = doubleSingleValueMatchCondition.QueryTag;
            var tagName = queryTag.Tag.ToString().Trim('(', ')');
            tagName = Regex.Replace(tagName, ",", "");
            var tagValue = doubleSingleValueMatchCondition.Value;
            var condition = $"(c['value']['{tagName}']['Value'][0] = {tagValue})";

            // add to query
            AddToQuery(condition);
            //throw new NotImplementedException();
        }

        public override void Visit(LongRangeValueMatchCondition longRangeValueMatchCondition)
        {
            // *** TODO *** based on conformance statement, no ranged search on longs ????
            throw new NotImplementedException();

        }

        public override void Visit(LongSingleValueMatchCondition longSingleValueMatchCondition)
        {
            EnsureArg.IsNotNull(longSingleValueMatchCondition, nameof(longSingleValueMatchCondition));
            var queryTag = longSingleValueMatchCondition.QueryTag;
            var tagName = queryTag.Tag.ToString().Trim('(', ')');
            tagName = Regex.Replace(tagName, ",", "");
            var tagValue = longSingleValueMatchCondition.Value;
            var condition = $"(c['value']['{tagName}']['Value'][0] = {tagValue})";

            AddToQuery(condition);
            //throw new NotImplementedException();
        }
    }
}
