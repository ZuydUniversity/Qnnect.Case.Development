using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Qnnect.Case.AzureFunction
{
    public static class PhotoAnalyzerFunction
    {
        [FunctionName("PhotoAnalyzerFunction")]
        public static void Run([QueueTrigger("photoqueue")]string blobName, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {blobName}");

            var photoId = Guid.Parse(blobName);
        }
    }
}
