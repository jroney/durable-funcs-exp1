using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Company.Function
{
    public static class HelloOrchestration
    {
        [FunctionName("HelloOrchestration")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "London"));

            var testPolyOutput = await context.CallActivityAsync<BaseFoo>(nameof(TestOutputWithInheritance), "");
            outputs.Add(testPolyOutput.GetType().FullName);
            // var testPolyOutput = await context.CallActivityAsync<JObject>(nameof(TestOutputWithInheritance), "");

            // var bar = testPolyOutput.ToObject<Bar>();
            // outputs.Add(bar.GetType().FullName);
            // outputs.Add(bar.BarVal);

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName(nameof(SayHello))]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation("Saying hello to {name}.", name);
            return $"Hello {name}!";
        }

        [FunctionName(nameof(TestOutputWithInheritance))]
        public static async Task<BaseFoo> TestOutputWithInheritance([ActivityTrigger] string ignore, ILogger log)
        {
            await Task.Delay(1000);

            return new DerrivedFoo { BaseVal = "some base", DerrivedVal = "some derrived" };
        }

        [FunctionName("HelloOrchestration_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("HelloOrchestration", null);

            log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }

    public class BaseFoo
    {
        public string BaseVal { get; set; }
    }

    public class DerrivedFoo : BaseFoo
    {
        public string DerrivedVal { get; set; }
    }
}