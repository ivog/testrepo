
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.ServiceBus;

namespace Company.Function
{
    public static class HttpTrigger
    {
        [FunctionName("HttpTrigger")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, 
            [EventHub("deloittedemoeventhub", Connection = "EventHubConnectionString")] ICollector<Order> outputEventHubMessages,
            [Table("Order", Connection = "AzureWebJobsDashboard")] ICollector<Order> outTable,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            var order = new Order{ PizzaType = data?.pizzaType, Amount = data?.amount, PartitionKey = "Functions", RowKey = System.Guid.NewGuid().ToString() };

            outputEventHubMessages.Add(order);
            outTable.Add(order);

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
