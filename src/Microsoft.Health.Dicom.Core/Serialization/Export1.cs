// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------



//using System;
//using System.Collections;

//namespace Microsoft.Health.Dicom.Core.Serialization
//{
//    public class Export1
//    {
//        public class StudyInput
//        {
//            public string StudyId { get; set; }
//        }
//        public class SeriesInput
//        {
//            public string StudyId { get; set; }
//            public string SeriesId { get; set; }
//        }
//        public class InstanceInput
//        {
//            public string StudyId { get; set; }
//            public string SeriesId { get; set; }
//            public string Instance { get; set; }
//        }
//        public void parseString(string input)
//        {
//            // No slases it is one study "daasdsadasdasd" -> study
//            //one slash -> "dsadsadsad/asdsadsad  -> series
//        }


//        ///This class the only we pass to THe Azure Function
//        public class FinalInputToWill
//        {
//            public IList<StudyInput> studies;
//            public IList<SeriesInput> series;
//            public IList<InstanceInput> instances;
//        }
//        public class AzureStorageDestination
//        {
//            public Uri Uri { get; set; }
//        }
//        // Request Body (simple)
//        public class Payload
//        {

//        }
//    }
//}

//*
// * {
//    source: {
//        idFilter: {
//            ids: [
//                "studyUid",
//                "studyUid/SeriesUid",
//                "studyUid/SeriesUid/InstanceUid"
//              ]
//        }
//    }
//    destination: {
//        azureStorage: {
//            uri: "https://foobar.blob.core.windows.net/exportcontainer"
//        }
//    }
//}
// */

