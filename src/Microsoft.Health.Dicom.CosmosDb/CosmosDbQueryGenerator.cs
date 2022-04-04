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
                _query += " AND ";
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

            // get the tag
            var tagName = FormatTagString(rangeValueMatchCondition.QueryTag);

            var fromDate = rangeValueMatchCondition.Minimum;
            var toDate = rangeValueMatchCondition.Maximum;

            // match on formatting written to cosmos
            var fromDateFormatted = fromDate.ToString("yyyyMMdd");
            var toDateFormatted = toDate.ToString("yyyyMMdd");

            string condition = $"(c['value']['{tagName}']['Value'][0] BETWEEN '{fromDateFormatted}' AND '{toDateFormatted}')";

            AddToQuery(condition);
        }

        public override void Visit(DateSingleValueMatchCondition dateSingleValueMatchCondition)
        {
            // Ensure the condition is not null
            EnsureArg.IsNotNull(dateSingleValueMatchCondition, nameof(dateSingleValueMatchCondition));

            // get the tag and convert the value to match the date format on the dicom storage
            var tagName = FormatTagString(dateSingleValueMatchCondition.QueryTag);
            string tagValue = dateSingleValueMatchCondition.Value.ToString("yyyyMMdd");
            string condition = $"(c['value']['{tagName}']['Value'][0] = '{tagValue}')";

            AddToQuery(condition);
        }

        public override void Visit(PersonNameFuzzyMatchCondition fuzzyMatchCondition)
        {
            // Ensure the condition is not null
            EnsureArg.IsNotNull(fuzzyMatchCondition, nameof(fuzzyMatchCondition));
            var tagName = FormatTagString(fuzzyMatchCondition.QueryTag);
            var tagValue = fuzzyMatchCondition.Value.ToString();

            // Break up the string into tokens of names, and do regex matching on each part
            var nameWords = tagValue.Split(' ');
            var nameWordsLength = nameWords.Length;
            for (var i = 0; i < nameWordsLength; i++)
            {
                nameWords[i] = $"{nameWords[i]}.*";
            }
            var condition = $"(REGEXMATCH(c['value']['{tagName}']['Value'][0]['Alphabetic'], '{String.Join("", nameWords)}', 'i'))";

            AddToQuery(condition);
        }

        public override void Visit(DoubleSingleValueMatchCondition doubleSingleValueMatchCondition)
        {
            // Ensure the condition is not null
            EnsureArg.IsNotNull(doubleSingleValueMatchCondition, nameof(doubleSingleValueMatchCondition));
            // get the tag & value
            var tagName = FormatTagString(doubleSingleValueMatchCondition.QueryTag);
            var tagValue = doubleSingleValueMatchCondition.Value;
            var condition = $"(c['value']['{tagName}']['Value'][0] = {tagValue})";

            AddToQuery(condition);
        }

        public override void Visit(LongRangeValueMatchCondition longRangeValueMatchCondition)
        {
            throw new NotImplementedException();
        }

        public override void Visit(LongSingleValueMatchCondition longSingleValueMatchCondition)
        {
            // Ensure the condition is not null
            EnsureArg.IsNotNull(longSingleValueMatchCondition, nameof(longSingleValueMatchCondition));
            // Get the tag and the value
            var tagName = FormatTagString(longSingleValueMatchCondition.QueryTag);
            var tagValue = longSingleValueMatchCondition.Value;
            var condition = $"(c['value']['{tagName}']['Value'][0] = {tagValue})";

            AddToQuery(condition);
        }

    }
}
