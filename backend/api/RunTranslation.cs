using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Text;

namespace Company.Function
{
    public static class RunTranslation
    {
        [FunctionName("RunTranslation")]
        public static HttpResponseMessage Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            log.LogInformation(requestBody);
            dynamic translationRequest = JsonConvert.DeserializeObject(requestBody);

            var translationResponse = new{
                _id = -1,
                text_in = translationRequest.text_in,
                text_out = "translationRequest.text_out",
                lang_in = translationRequest.lang_in,
                lang_out = translationRequest.lang_out
            };
            
            // Serialize the response object to JSON
            string jsonResponse = JsonConvert.SerializeObject(translationResponse);
            log.LogInformation($"Response JSON: {jsonResponse}");

            // Return the JSON response
            HttpResponseMessage Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json"),
                StatusCode = System.Net.HttpStatusCode.OK,
            };
            return Response;
        }
    }
}
