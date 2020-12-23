using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Identity.Client;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Azure.Core;
using Azure.Identity;

namespace MyAadFunction
{
    public static class GetAadUser
    {
        [FunctionName("GetAadUser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            //string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";

            Config config = Config.ReadFromJsonFile("appsettings.json");

            var app = new DefaultAzureCredential();

            string[] scopes = new string[] { $"{config.ApiUrl}.default" };

            AccessToken accessToken;

            try
            {
                var tokenContext = new TokenRequestContext(scopes);
                var cancellationToken = new CancellationToken(default);

                accessToken = await app.GetTokenAsync(tokenContext, cancellationToken);                
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                throw;
            }

            JObject outcome = null;
            if (accessToken.Token != null)
            {
                var httpClient = new HttpClient();
                var apiCaller = new ProtectedApiCallHelper(httpClient);
                outcome = await apiCaller.CallWebApiAndProcessResultASync($"{config.ApiUrl}v1.0/users", accessToken.Token);
            }
            else
            {
                return new UnauthorizedResult();
            }
            return new OkObjectResult(outcome);
        }
    }
}
