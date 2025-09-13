using System.Net.Sockets;

namespace Broker.Networking;

public class ConnectionInfo
{
    public byte[] Buffer  { get; set; }
    public Socket Socket  { get; set; }
    public string Address { get; set; }
    public string Topic   { get; set; }

    public ConnectionInfo()
    {
        Buffer = new byte[Constants.BufferSize];
    }
}