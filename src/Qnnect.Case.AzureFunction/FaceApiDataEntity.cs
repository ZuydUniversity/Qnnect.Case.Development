using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System;

namespace Qnnect.Case.AzureFunction
{
    public class FaceApiDataEntity : TableEntity
    {
        public Guid PhotoId { get; set; }

        public string AnalysisResult { get; set; }

        public FaceApiDataEntity(Guid photoId, JObject analysisResult)
        {
            PartitionKey = "Qnnect";
            RowKey = photoId.ToString();
            PhotoId = photoId;
            AnalysisResult = analysisResult.ToString();
        }
    }
}