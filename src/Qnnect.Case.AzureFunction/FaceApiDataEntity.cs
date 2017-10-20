using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;

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
