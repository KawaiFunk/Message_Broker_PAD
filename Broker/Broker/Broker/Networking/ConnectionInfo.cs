// ConnectionInfo.cs
using System.Net.Sockets;
using Broker.Payloads.Enums;

namespace Broker.Networking;

public class ConnectionInfo
{
    public byte[] Buffer  { get; set; }
    public Socket Socket  { get; set; }
    public string Address { get; set; }
    public string Topic   { get; set; }

    // Default JSON unless client says otherwise
    public MessageFormat Format { get; set; } = MessageFormat.Json;

    // NEW: carry partial data between BeginReceive calls
    public string Pending { get; set; } = string.Empty;

    public ConnectionInfo()
    {
        Buffer = new byte[Constants.BufferSize];
    }
}