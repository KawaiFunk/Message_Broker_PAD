using System.Net.Sockets;
using Broker.Payloads.Enums;

namespace Broker.Networking;

public class ConnectionInfo
{
    public byte[] Buffer  { get; set; }
    public Socket Socket  { get; set; }
    public string Address { get; set; }
    public string Topic   { get; set; }

    public MessageFormat Format { get; set; } = MessageFormat.Json;
    public string Pending  { get; set; } = string.Empty;

    public object SendLock { get; }  = new();

    public ConnectionInfo()
    {
        Buffer = new byte[Constants.BufferSize];
    }
}