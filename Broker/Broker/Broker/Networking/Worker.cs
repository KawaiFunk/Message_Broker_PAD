// Broker/Networking/Worker.cs
using System.Text;
using System.Net.Sockets;
using Broker.Payloads;
using Broker.Payloads.Enums;
using Broker.Payloads.Helpers; // Serialize(...) lives here
using Broker.Storage;

namespace Broker.Networking;

public class Worker
{
    /// <summary>
    /// Start consuming payloads and fanning them out to all subscribers of the payload.Topic.
    /// This method blocks until PayloadStorage.Complete() is called and the queue drains.
    /// </summary>
    public void DoSendMessageWork()
    {
        Console.WriteLine("Dispatcher worker started.");

        foreach (var payload in PayloadStorage.GetConsumingEnumerable())
        {
            if (payload is null) continue;

            var targets = ConnectionsStorage.GetConnectionsByTopic(payload.Topic);
            if (targets.Count == 0) continue;

            foreach (var conn in targets)
            {
                try
                {
                    // Serialize in the SUBSCRIBER'S preferred format and add newline framing
                    string text = PayloadSerializer.Serialize(payload, conn.Format) + "\n";
                    byte[] bytes = Encoding.UTF8.GetBytes(text);

                    // Prevent interleaved writes to the same socket
                    lock (conn.SendLock)
                    {
                        SendAll(conn.Socket, bytes);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Send to {conn.Address} failed: {ex.Message} — removing connection.");
                    ConnectionsStorage.RemoveConnection(conn.Address);
                    try { conn.Socket.Close(); } catch { /* ignore */ }
                }
            }
        }

        Console.WriteLine("Dispatcher worker stopped (payload queue completed).");
    }

    private static void SendAll(Socket socket, byte[] data)
    {
        int offset = 0;
        while (offset < data.Length)
        {
            int sent = socket.Send(data, offset, data.Length - offset, SocketFlags.None);
            if (sent <= 0) throw new IOException("Socket send returned 0/negative.");
            offset += sent;
        }
    }
}
