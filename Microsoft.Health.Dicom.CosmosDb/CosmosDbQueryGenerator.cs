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
using Microsoft.Health.Dicom.Core.Features.ExtendedQueryTag;
using Microsoft.Health.Dicom.Core.Features.Query;
using System.Text.RegularExpressions;

namespace Microsoft.Health.Dicom.CosmosDb
{
    public class CosmosDbQueryGenerator : QueryFilterConditionVisitor
    {
        private string _query = "";

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

        private static string FormatTagString(QueryTag queryTag)
        {
            // initally Query Tags are formatted as "(####,####)"
            var tagName = queryTag.Tag.ToString().Trim('(', ')');
            tagName = Regex.Replace(tagName, ",", "");
            return tagName;
        }

        public string OutputQuery()
        {
            return _query;
        }

        public override void Visit(StringSingleValueMatchCondition stringSingleValueMatchCondition)
        {
            EnsureArg.IsNotNull(stringSingleValueMatchCondition, nameof(stringSingleValueMatchCondition));
            string tagName = FormatTagString(stringSingleValueMatchCondition.QueryTag);
            string tagValue = stringSingleValueMatchCondition.Value;
            string condition = $"(c['value']['{tagName}']['Value'][0] = '{tagValue}')";
            AddToQuery(condition);
        }

        public override void Visit(DateRangeValueMatchCondition rangeValueMatchCondition)
        {
            // Ensure not null
            EnsureArg.IsNotNull(rangeValueMatchCondition, nameof(rangeValueMatchCondition));

            //get the tag
            // *** TODO *** limit the date comparisons to the two tags supported currently
            var tagName = FormatTagString(rangeValueMatchCondition.QueryTag);

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
            var tagName = FormatTagString(dateSingleValueMatchCondition.QueryTag);
            string tagValue = dateSingleValueMatchCondition.Value.ToString("yyyyMMdd");
            string condition = $"(c['value']['{tagName}']['Value'][0] = '{tagValue}')";

            AddToQuery(condition);
        }

        public override void Visit(PersonNameFuzzyMatchCondition fuzzyMatchCondition)
        {
            EnsureArg.IsNotNull(fuzzyMatchCondition, nameof(fuzzyMatchCondition));
            var tagName = FormatTagString(fuzzyMatchCondition.QueryTag);
            var tagValue = fuzzyMatchCondition.Value.ToString(); // **** TODO **** change this to be matching value or something

            //f true fuzzy matching is applied to PatientName attribute.
            //It will do a prefix word match of any name part inside PatientName value.
            //For example, if PatientName is "John^Doe",
            //          then "joh", "do", "jo do", "Doe" and "John Doe" will all match.
            //However "ohn" will not match

            // split on spaces and CONTAINS(slksjf) OR contains(sjifiwoe)
            // REGEX MATCH?
            var nameWords = tagValue.Split(' ');
            for (var i = 0; i < nameWords.Length; i++)
            {
                //condition += $"(CONTAINS(c['value']['{tagName}']['Value'][0]['Alphabetic']), '{name}')";
                var searchName = nameWords[i];
                nameWords[i] = $"(CONTAINS(c['value']['{tagName}']['Value'][0]['Alphabetic'], '{searchName}', true))";
            }

            var condition = "(" + String.Join(" OR ", nameWords) + ")"; // ***TODO*** and????

            //var condition = $"(STARTSWITH(c['value']['{tagName}']['Value'][0]['Alphabetic'], '{tagValue}'))";

            AddToQuery(condition);
        }

        public override void Visit(DoubleSingleValueMatchCondition doubleSingleValueMatchCondition)
        {
            //Ensure the condition is not null
            EnsureArg.IsNotNull(doubleSingleValueMatchCondition, nameof(doubleSingleValueMatchCondition));
            //doubleSingleValueMatchCondition;
            // get the tag & value
            var tagName = FormatTagString(doubleSingleValueMatchCondition.QueryTag);
            var tagValue = doubleSingleValueMatchCondition.Value;
            var condition = $"(c['value']['{tagName}']['Value'][0] = {tagValue})";

            // add to query
            AddToQuery(condition);
        }

        public override void Visit(LongRangeValueMatchCondition longRangeValueMatchCondition)
        {
            // *** TODO *** based on conformance statement, no ranged search on longs ????
            throw new NotImplementedException();

        }

        public override void Visit(LongSingleValueMatchCondition longSingleValueMatchCondition)
        {
            EnsureArg.IsNotNull(longSingleValueMatchCondition, nameof(longSingleValueMatchCondition));
            var tagName = FormatTagString(longSingleValueMatchCondition.QueryTag);
            var tagValue = longSingleValueMatchCondition.Value;
            var condition = $"(c['value']['{tagName}']['Value'][0] = {tagValue})";
            //var condition = $"(c.value.{tagName}.Value[0] = {tagValue})";


            AddToQuery(condition);
        }
    }
}
