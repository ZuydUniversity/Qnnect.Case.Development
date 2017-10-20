using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace Qnnect.Case.AzureFunction
{
    public static class PhotoAnalyzerFunction
    {
        //The subscription key needed in order to authorize against the Microsoft Face detect API.
        private const string SubscriptionKey = "[YOURSUBSCRIPTIONKEY]";
        //The base Uri for the Microsoft Face detect API.
        private const string UriBase = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/detect";

        [FunctionName("PhotoAnalyzerFunction")]
        public static async Task Run(
        [QueueTrigger("photoqueue")]string blobName, //This QueueTriggerAttribute listens to a queue called 'photoqueue' on our Azure Storage account. This will cause this Function to trigger whenever a message is posted on that queue and put the message content (in our case a GUID reference of the uploaded photo) into the 'blobName' parameter.
        [Blob("photos/{queueTrigger}")] CloudBlockBlob cloudBlockBlob, //This BlobAttribute looks for a blob which matches the QueueTrigger message content in a blob container called 'photos' and puts a reference to that blob into the 'cloudBlockBlob' paramter which can be used inside the Function.
        [Table("faceapimetadata")] CloudTable table, //This TableAttribute will provide us with a CloudTable reference for a table called 'faceapimetadata' which we can use to store the metadata retrieved from the Microsoft Face detect API.
        TraceWriter log)
        {
            //Log the blobName to console.
            log.Info($"Processing blobName '{blobName}'");

            //Parse the recieved blobName to a Guid.
            var photoId = Guid.Parse(blobName);

            //Download the actual photo bytes as a byte array.
            var byteArray = new byte[cloudBlockBlob.StreamWriteSizeInBytes];
            cloudBlockBlob.DownloadToByteArray(byteArray, 0);

            //Pass the photo byte array to the MakeAnalysisRequest method.
            var analysisResult = await MakeAnalysisRequest(byteArray);

            if (analysisResult == null || !analysisResult.Any())
            {
                //if the Microsoft Face detect API did not return any information, log this to console.
                log.Info($"No face detected in photo with Id {photoId}.");
                return;
            }

            //Save the analysis JSON result to Azure Table Storage. Use the first result since we want only one face and there is no way to know which one is correct when multiple.
            await SaveAnalysisResultToStorage(photoId, JObject.Parse(analysisResult.First().ToString()), table);

            //Log the JSON result to console.
            log.Info("\nResult:\n");
            log.Info(analysisResult.ToString(Formatting.Indented));
        }

        /// <summary>
        /// Gets the analysis of the specified photo file by using the Computer Vision REST API.
        /// </summary>
        /// <param name="photoBytes">The photo file.</param>
        private static async Task<JArray> MakeAnalysisRequest(byte[] photoBytes)
        {
            var client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);

            // Request parameters. This contains the attributes you expect to be returned from the Microsoft Face detect API. (see: https://westeurope.dev.cognitive.microsoft.com/docs/services/563879b61984550e40cbbe8d/operations/563879b61984550f30395236 for the API Reference.
            const string requestParameters = "returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

            // Assemble the URI for the REST API Call.
            var uri = $"{UriBase}?{requestParameters}";

            using (var content = new ByteArrayContent(photoBytes))
            {
                // Set the request content type to be compatible with our byte array.
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call against the Microsoft Face detect API.
                var response = await client.PostAsync(uri, content);

                // Get the JSON response as string.
                var contentString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    //Parse the error content as valid JSON (this could contain valuable error information for debugging purposes) and throw an appropriate exception.
                    throw new HttpException((int)response.StatusCode, string.IsNullOrWhiteSpace(contentString) ? null : JObject.Parse(contentString).ToString(Formatting.Indented));
                }

                //Parse the response content as valid JSON and return, or return null if the response content is empty (this is the case when the Microsoft Face detect API wasn't able to detect a face in the photo.
                return string.IsNullOrWhiteSpace(contentString) ? null : JArray.Parse(contentString);
            }
        }

        private static async Task SaveAnalysisResultToStorage(Guid photoId, JObject analysisResult, CloudTable table)
        {
            //Create an actual table based on the provided CloudTable reference if one does not already exists.
            await table.CreateIfNotExistsAsync();

            //Insert the analysisResult into the table and use the photoId as unique identifier.
            await table.ExecuteAsync(TableOperation.InsertOrReplace(new FaceApiDataEntity(photoId, analysisResult)));
        }
    }
}