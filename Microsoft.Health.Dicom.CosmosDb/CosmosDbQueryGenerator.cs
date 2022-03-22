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
                _query += "AND";
            }
            _query += val;
        }
        public override void Visit(DateRangeValueMatchCondition rangeValueMatchCondition)
        {
            // Ensure not null
            EnsureArg.IsNotNull(rangeValueMatchCondition, nameof(rangeValueMatchCondition));
            //*** TODO *** do we want Linq?

            // get the table/container
            //*** TODO *** do we need strong typing?
            //*** TODO *** do we need the equivalent of the DicomTageSqlEntry.cs?
            //*** TODO *** do we need equivalent of GetTableAlias?
            var tableAlias = "i am the container";
            //get the tag
            //*** TODO *** limit the date comparisons to the two tags supported currently
            var tagname = ""
            // pick out the two operands fromDate & toDate
            var fromDate = "20200922";
            var toDate = "20200923";

            string condition = "";

            if (!fromDate && !toDate)
            {
                // if neither are present, error
                throw new Exception("Providing no dates in range is not allowed. Please provide one or both the range bounds.");
            }
            else if (!fromDate && toDate)
            {
                // if from date present but not todate, get everrything after that date
            }
            else if (fromDate && !toDate)
            {
                // if to date present but not fromdate, get everything before this date
            }
            else
            {
                // use BETWEEN-equivalent
                // ***TODO*** do we ensure strong typing going in?
                // INCLUSIVE
                condition = $"(c[{tagName}] BETWEEN {fromDate} AND {toDate})";


                //INSPO:
                //IQueryable<Order> orders = container.GetItemLinqQueryable<Order>(allowSynchronousQueryExecution: true).Where(o => o.ShipDate >= DateTime.UtcNow.AddDays(-3));
            }


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
