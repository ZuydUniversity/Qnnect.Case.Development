using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Qnnect.Case.AzureFunction
{
    public static class PhotoAnalyzerFunction
    {
        private const string SubscriptionKey = "[YOURSUBSCRIPTIONKEY]";
        private const string UriBase = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/detect";

        [FunctionName("PhotoAnalyzerFunction")]
        public static async Task Run(
            [QueueTrigger("photoqueue")] string blobName,
            [Blob("photos/{queueTrigger}")] CloudBlockBlob cloudBlockBlob,
            TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {blobName}");

            var photoId = Guid.Parse(blobName);

            var byteArray = new byte[cloudBlockBlob.StreamWriteSizeInBytes];
            cloudBlockBlob.DownloadToByteArray(byteArray, 0);

            var analysisResult = await MakeAnalysisRequest(byteArray);

            if (analysisResult == null || !analysisResult.Any())
            {
                log.Info($"No face detected in photo with Id {photoId}.");
                return;
            }
        }

        private static async Task<JArray> MakeAnalysisRequest(byte[] photoBytes)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);

            const string requestParameters = "returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

            var uri = $"{UriBase}?{requestParameters}";

            using (var content = new ByteArrayContent(photoBytes))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var response = await client.PostAsync(uri, content);

                var contentString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpException((int)response.StatusCode, string.IsNullOrWhiteSpace(contentString) ? null : JObject.Parse(contentString).ToString(Formatting.Indented));
                }

                return string.IsNullOrWhiteSpace(contentString) ? null : JArray.Parse(contentString);
            }
        }
    }
}
