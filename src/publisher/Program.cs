using System;
using System.Threading.Tasks;
using System.Text;
using Azure.Messaging.EventHubs.Producer;
using Azure.Messaging.EventHubs;

namespace publisher
{
 class Program
    {

        private static string EventHubConnectionString;
    
        private static async Task Main(string[] args)
        {
            EventHubConnectionString = Environment.GetEnvironmentVariable("EH_AZD_EH_CONNECTION");

            if (String.IsNullOrEmpty(EventHubConnectionString))
            {
                Console.WriteLine("Environment variable EH_AZD_EH_CONNECTION does not specified");
                Environment.Exit(1);
            }

            var producer = new EventHubProducerClient(EventHubConnectionString, EventHubConnectionString.Split("EntityPath=")[1]);

            try
            {
                int batchNum = 1;
                while (true)
                {

                    await SendEventsToEventHub(producer, batchNum++, 10);
                    Console.WriteLine($"Press any key to send more events");
                    Console.ReadKey();
                               
                }
            }
            finally
            {
                await producer.CloseAsync();
            }          

        }

        // Creates an Event Hub client and sends messages to the event hub.
        private static async Task SendEventsToEventHub(EventHubProducerClient producer, int batchNum, int numMsgToSend)
        {
            using EventDataBatch eventBatch = await producer.CreateBatchAsync();

            for (var counter = 0; counter < numMsgToSend; ++counter)
            {
                var eventBody = new BinaryData($"Event Number: {counter} from batch {batchNum}");
                var eventData = new EventData(eventBody);

                if (!eventBatch.TryAdd(eventData))
                {
                    // At this point, the batch is full but our last event was not
                    // accepted.  For our purposes, the event is unimportant so we
                    // will intentionally ignore it.  In a real-world scenario, a
                    // decision would have to be made as to whether the event should
                    // be dropped or published on its own.

                    break;
                }
            }

            await producer.SendAsync(eventBatch);

            Console.WriteLine($"Batch of {numMsgToSend} events sent.");
        }
    }
}
