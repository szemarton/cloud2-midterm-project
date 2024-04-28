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
using Ganss.Xss;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using Microsoft.Azure.WebJobs.Extensions.Storage.Blobs;
using Azure.Storage.Blobs;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Company.Function
{
    public static class RunTranslation
    {
        private static readonly string accesskey = "AzureAIservicesAccessKey";
        private static readonly string location = "AzureAIservicesLocation";
        private static readonly string endpoint = "AzureAIservicesEndpoint";
        private static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        [FunctionName("RunTranslation")]
        public static HttpResponseMessage Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        [Blob("$web/json/history.json")] BlobClient blobHistory,
        [Blob("$web/json/history.json")] string history,
        ILogger log)
        {
            var sanitizer = new HtmlSanitizer();

            string requestBody = sanitizer.Sanitize(new StreamReader(req.Body).ReadToEnd());
            log.LogInformation(requestBody);
            dynamic translationRequest = JsonConvert.DeserializeObject(requestBody);
            string lang_in = sanitizer.Sanitize(translationRequest.lang_in.Value);
            string lang_out = sanitizer.Sanitize(translationRequest.lang_out.Value);
            string text_in = sanitizer.Sanitize(translationRequest.text_in.Value);

            // Input and output languages are defined as parameters.
            string route = $"/translate?api-version=3.0&from={lang_in}&to={lang_out}";
            string textToTranslate = text_in;
            if (textToTranslate.Length <= 256)
            {
                object[] body = new object[] { new { Text = textToTranslate } };
                var translateRequestBody = JsonConvert.SerializeObject(body);

                var client = new HttpClient();
                var request = new HttpRequestMessage();

                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(GetEnvironmentVariable(endpoint) + route);
                request.Content = new StringContent(translateRequestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", GetEnvironmentVariable(accesskey));
                // location required if you're using a multi-service or regional (not global) resource.
                request.Headers.Add("Ocp-Apim-Subscription-Region", GetEnvironmentVariable(location));

                // Send the request and get response.
                HttpResponseMessage response = client.Send(request);
                // Read response as a string.
                var result = new StreamReader(response.Content.ReadAsStream()).ReadToEnd();
                dynamic translation = JsonConvert.DeserializeObject(result);
                string text_out = translation[0].translations[0].text;
                log.LogInformation(result);

                dynamic translationResponse = new
                {
                    _id = -1,
                    text_in = text_in,
                    text_out = text_out,
                    lang_in = lang_in,
                    lang_out = lang_out
                };

                List<dynamic> existingTranslations = new List<dynamic>();
                if (blobHistory.Exists())
                {
                    using (var stream = new MemoryStream())
                    {
                        blobHistory.DownloadTo(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        using (var reader = new StreamReader(stream))
                        {
                            string existingJson = reader.ReadToEnd();
                            existingTranslations = JsonConvert.DeserializeObject<List<dynamic>>(existingJson);
                        }
                    }

                    dynamic newhistoryrecord = new
                    {
                        _id = existingTranslations.Count,
                        text_in = text_in,
                        text_out = text_out,
                        lang_in = lang_in,
                        lang_out = lang_out
                    };

                    existingTranslations.Add(newhistoryrecord);
                }


                // Serialize the updated translations to JSON
                string updatedJson = JsonConvert.SerializeObject(existingTranslations);

                // Upload the updated JSON to the blob
                byte[] byteArray = Encoding.UTF8.GetBytes(updatedJson);
                using (MemoryStream stream = new MemoryStream(byteArray))
                {
                    blobHistory.Upload(stream, true);
                }

                // Serialize the response object to JSON
                string jsonResponse = JsonConvert.SerializeObject(translationResponse);
                log.LogInformation($"Response JSON: {jsonResponse}");

                // Return the JSON responBlobStse
                HttpResponseMessage Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json"),
                    StatusCode = System.Net.HttpStatusCode.OK,
                };
                return Response;
            }
            else
            {
                // Return the JSON response
                HttpResponseMessage Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"error\" : \"too much text\"}", Encoding.UTF8, "application/json"),
                    StatusCode = System.Net.HttpStatusCode.OK,
                };
                return Response;
            }


        }
    }
}
