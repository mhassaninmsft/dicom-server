// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------



namespace Microsoft.Health.Dicom.Core.Features.Export.Model;
public class Destination
{
    //
    public StorageAccountConfiguration StorageAccountConfiguration { get; set; }
    public CosmosConfiguration CosmosConfiguration { get; set; }
}



/*
 {
   'destination':{
'StorageAccountConfiguration':{
     'ConnectionString':'www.bbno.com'
}
  }
 }
 */

/*
 {
   'destination':{
'CosmosConfiguration':{
     'PrimaryKey':'dsadsadas',
     'AccountName':'MyDB'
}
  }
 }
 */




public class StorageAccountConfiguration
{
    public string ConnectionString { get; set; }
}
public class CosmosConfiguration
{
    private string PrimaryKey { get; set; }
    public string AccountName { get; set; }
}
