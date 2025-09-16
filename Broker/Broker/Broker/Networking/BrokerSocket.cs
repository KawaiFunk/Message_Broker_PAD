using System.Net;
using System.Net.Sockets;
using System.Text;
using Broker.Payloads;
using Broker.Storage;

namespace Broker.Networking;

public class BrokerSocket
{
    private readonly Socket _socket = new Socket(
        AddressFamily.InterNetwork,
        SocketType.Stream,
        ProtocolType.Tcp);

    public void Start(string ipAddress, int port)
    {
        _socket.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), port));
        _socket.Listen(Constants.ConnectionQueueSize);
        
        var worker = new Worker();
        _ = Task.Run(() => worker.DoSendMessageWork());

        Console.WriteLine($"Broker started on {ipAddress}:{port}");
        AcceptConnection();
    }

    private void AcceptConnection()
    {
        _socket.BeginAccept(AcceptedCallback, null);
    }

    private void AcceptedCallback(IAsyncResult result)
    {
        var connection = new ConnectionInfo();

        try
        {
            connection.Socket  = _socket.EndAccept(result);
            connection.Address = connection.Socket.RemoteEndPoint?.ToString() ?? "Unknown";
            Console.WriteLine($"New client connected: {connection.Address}");

            connection.Socket.BeginReceive(
                connection.Buffer,
                0,
                Constants.BufferSize,
                SocketFlags.None,
                RecievedCallback,
                connection
            );
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to accept connection: {e.Message}");
            if (connection.Socket != null)
            {
                try { connection.Socket.Close(); }
                catch
                {
                    // ignored
                }
            }
        }
        finally
        {
            AcceptConnection();
        }
    }

    private void RecievedCallback(IAsyncResult result)
{
    var connection = result.AsyncState as ConnectionInfo;
    if (connection?.Socket == null) return;

    var clientAddress = connection.Address;

    try
    {
        var socket = connection.Socket;

        if (!socket.Connected)
        {
            Console.WriteLine($"Client already disconnected: {clientAddress}");
            CleanupConnection(connection, clientAddress);
            return;
        }

        SocketError response;
        int bufferSize = socket.EndReceive(result, out response);

        if (response != SocketError.Success || bufferSize == 0)
        {
            Console.WriteLine($"Client disconnected: {clientAddress}");
            CleanupConnection(connection, clientAddress);
            return;
        }

        string chunk = Encoding.UTF8.GetString(connection.Buffer, 0, bufferSize);
        string aggregate = connection.Pending + chunk;

        int start = 0;
        while (true)
        {
            int newline = aggregate.IndexOf('\n', start);
            if (newline < 0) break; 

            string line = aggregate.Substring(start, newline - start).TrimEnd('\r');
            if (!string.IsNullOrWhiteSpace(line))
            {
                PayloadHandler.HandlePayload(Encoding.UTF8.GetBytes(line), connection);
            }
            start = newline + 1;
        }

        connection.Pending = aggregate.Substring(start);

        socket.BeginReceive(
            connection.Buffer,
            0,
            Constants.BufferSize,
            SocketFlags.None,
            RecievedCallback,
            connection
        );
    }
    catch (ObjectDisposedException)
    {
        Console.WriteLine($"Socket already disposed for client: {clientAddress}");
        ConnectionsStorage.RemoveConnection(clientAddress);
    }
    catch (SocketException ex)
    {
        Console.WriteLine($"Socket error for client {clientAddress}: {ex.Message}");
        CleanupConnection(connection, clientAddress);
    }
    catch (Exception e)
    {
        Console.WriteLine($"Failed to receive data from {clientAddress}: {e.Message}");
        CleanupConnection(connection, clientAddress);
    }
}


    private void CleanupConnection(ConnectionInfo connection, string clientAddress)
    {
        try
        {
            ConnectionsStorage.RemoveConnection(clientAddress);
            
            if (connection.Socket != null)
            {
                if (connection.Socket.Connected)
                {
                    connection.Socket.Shutdown(SocketShutdown.Both);
                }
                connection.Socket.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during connection cleanup for {clientAddress}: {ex.Message}");
        }
    }
}