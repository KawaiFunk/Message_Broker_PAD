using System.Text;
using Broker.Networking;
using Broker.Payloads.Enums;
using Broker.Payloads.Helpers;
using Broker.Storage;
using Newtonsoft.Json;

namespace Broker.Payloads;

public static class PayloadHandler
{
    public static void HandlePayload(byte[] payloadBytes, ConnectionInfo connectionInfo)
    {
        if (payloadBytes == null || payloadBytes.Length == 0)
        {
            Console.WriteLine("Received empty payload");
            return;
        }

        var payloadString = Encoding.UTF8.GetString(payloadBytes).TrimEnd('\r');

        if (string.IsNullOrWhiteSpace(payloadString))
        {
            Console.WriteLine("Received whitespace-only payload");
            return;
        }

        Console.WriteLine($"Received payload: {payloadString}");
        
        if (payloadString.StartsWith("FORMAT:", StringComparison.OrdinalIgnoreCase))
        {
            var fmt = payloadString.Substring("FORMAT:".Length).Trim();
            connectionInfo.Format = fmt.Equals("xml", StringComparison.OrdinalIgnoreCase)
                ? MessageFormat.Xml
                : MessageFormat.Json;

            Console.WriteLine($"[{connectionInfo.Address}] Set format to: {connectionInfo.Format}");
            return;
        }

        if (payloadString.Equals("SWITCHTYPE", StringComparison.OrdinalIgnoreCase))
        {
            connectionInfo.Format = connectionInfo.Format == MessageFormat.Json
                ? MessageFormat.Xml
                : MessageFormat.Json;

            Console.WriteLine($"[{connectionInfo.Address}] Switched format to: {connectionInfo.Format}");
            return;
        }

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
            return;
        }

        static MessageFormat DetectFormat(string text, MessageFormat fallback)
        {
            var span = text.AsSpan().TrimStart();
            if (span.IsEmpty) return fallback;
            return span[0] == '<' ? MessageFormat.Xml
                 : span[0] == '{' || span[0] == '[' ? MessageFormat.Json
                 : fallback;
        }

        var formatToUse = DetectFormat(payloadString, connectionInfo.Format);

        try
        {
            var payload = PayloadSerializer.Deserialize(payloadString, formatToUse);

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
            Console.WriteLine($"[{connectionInfo.Address}] Added message to queue - Topic: {payload.Topic}, Message: {payload.Message}");
        }
        catch (InvalidOperationException ex) when (formatToUse == MessageFormat.Xml)
        {
            Console.WriteLine($"XML error from {connectionInfo.Address}: {ex.InnerException?.Message ?? ex.Message}");
            Console.WriteLine($"Payload was: {payloadString}");
        }
        catch (JsonReaderException ex)
        {
            Console.WriteLine($"Invalid JSON from {connectionInfo.Address}: {ex.Message}");
            Console.WriteLine($"Payload was: {payloadString}");
        }
        catch (JsonSerializationException ex)
        {
            Console.WriteLine($"JSON serialization error from {connectionInfo.Address}: {ex.Message}");
            Console.WriteLine($"Payload was: {payloadString}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error processing payload from {connectionInfo.Address}: {ex.Message}");
            Console.WriteLine($"Payload was: {payloadString}");
        }
    }
}
