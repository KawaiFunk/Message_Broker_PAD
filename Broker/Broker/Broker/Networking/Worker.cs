using System.Text;
using Broker.Payloads;
using Broker.Storage;
using Newtonsoft.Json;

namespace Broker.Networking;

public class Worker
{
    public void DoSendMessageWork()
    {
        while (true)
        {
            while (!PayloadStorage.IsEmpty())
            {
                var payload = PayloadStorage.GetNextPayload();

                if (payload != null)
                {
                    var connections = ConnectionsStorage.GetConnectionsByTopic(payload.Topic);
                    foreach (var connection in connections)
                    {
                        var payloadString = JsonConvert.SerializeObject(payload);
                        var payloadBytes  = Encoding.UTF8.GetBytes(payloadString);

                        connection.Socket.Send(payloadBytes);
                    }
                }
            }

            Thread.Sleep(TimeSpan.FromSeconds(Constants.WorkerSleepTimeSec));
        }
    }
}