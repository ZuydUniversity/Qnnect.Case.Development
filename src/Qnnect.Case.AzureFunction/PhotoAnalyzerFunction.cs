using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Qnnect.Case.AzureFunction
{
    public static class PhotoAnalyzerFunction
    {
        [FunctionName("PhotoAnalyzerFunction")]
        public static void Run(
        [QueueTrigger("photoqueue")]string blobName,
        [Blob("photos/{queueTrigger}")] CloudBlockBlob cloudBlockBlob,
        TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {blobName}");

            var photoId = Guid.Parse(blobName);

            var byteArray = new byte[cloudBlockBlob.StreamWriteSizeInBytes];
            cloudBlockBlob.DownloadToByteArray(byteArray, 0);
        }
    }
}
