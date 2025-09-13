using System.Text;
using Broker.Networking;
using Broker.Storage;
using Newtonsoft.Json;

namespace Broker.Payloads;

public static class PayloadHandler
{
    public static void HandlePayload(byte[] payloadBytes, ConnectionInfo connectionInfo)
    {
        // Validate input
        if (payloadBytes == null || payloadBytes.Length == 0)
        {
            Console.WriteLine("Received empty payload");
            return;
        }

        var payloadString = Encoding.UTF8.GetString(payloadBytes).Trim();
        
        if (string.IsNullOrWhiteSpace(payloadString))
        {
            Console.WriteLine("Received whitespace-only payload");
            return;
        }

        Console.WriteLine($"Received payload: {payloadString}");

        // Handle subscription messages
        if (payloadString.StartsWith("SUBSCRIBE:", StringComparison.OrdinalIgnoreCase))
        {
            var topic = payloadString.Substring("SUBSCRIBE:".Length).Trim();
            
            if (string.IsNullOrWhiteSpace(topic))
            {
                Console.WriteLine($"Client {connectionInfo.Address} attempted to subscribe to empty topic");
                return;
            }
            
            connectionInfo.Topic = topic;
            ConnectionsStorage.AddConnection(connectionInfo);
            Console.WriteLine($"Client {connectionInfo.Address} subscribed to topic: {topic}");
        }
        else
        {
            // Handle JSON messages
            try
            {
                var payload = JsonConvert.DeserializeObject<Payload>(payloadString);
                
                if (payload == null)
                {
                    Console.WriteLine($"Failed to deserialize payload from {connectionInfo.Address}: null result");
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(payload.Topic))
                {
                    Console.WriteLine($"Received message from {connectionInfo.Address} with empty topic");
                    return;
                }
                
                PayloadStorage.AddPayload(payload);
                Console.WriteLine($"Added message to queue - Topic: {payload.Topic}, Message: {payload.Message}");
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine($"Invalid JSON from {connectionInfo.Address}: {ex.Message}");
                Console.WriteLine($"Payload was: {payloadString}");
            }
            catch (JsonSerializationException ex)
            {
                Console.WriteLine($"JSON serialization error from {connectionInfo.Address}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error processing payload from {connectionInfo.Address}: {ex.Message}");
            }
        }
    }
}