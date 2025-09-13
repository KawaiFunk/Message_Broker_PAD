using System.Net;
using System.Net.Sockets;
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

        var  clientAddress           = connection.Address;
        bool shouldContinueReceiving = true;

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
            var         bufferSize = socket.EndReceive(result, out response);

            if (response != SocketError.Success || bufferSize == 0)
            {
                Console.WriteLine($"Client disconnected: {clientAddress}");
                CleanupConnection(connection, clientAddress);
                return;
            }

            var payload = new byte[bufferSize];
            Array.Copy(connection.Buffer, payload, bufferSize);

            PayloadHandler.HandlePayload(payload, connection);
        }
        catch (ObjectDisposedException)
        {
            Console.WriteLine($"Socket already disposed for client: {clientAddress}");
            shouldContinueReceiving = false;
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Socket error for client {clientAddress}: {ex.Message}");
            shouldContinueReceiving = false;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to receive data from {clientAddress}: {e.Message}");
            shouldContinueReceiving = false;
        }

        if (shouldContinueReceiving && connection.Socket != null)
        {
            try
            {
                if (connection.Socket.Connected && !connection.Socket.Poll(0, SelectMode.SelectRead))
                {
                    connection.Socket.BeginReceive(
                        connection.Buffer,
                        0,
                        Constants.BufferSize,
                        SocketFlags.None,
                        RecievedCallback,
                        connection
                    );
                }
                else
                {
                    Console.WriteLine($"Socket not ready for receiving, disconnecting: {clientAddress}");
                    CleanupConnection(connection, clientAddress);
                }
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine($"Socket disposed while trying to continue receiving: {clientAddress}");
                ConnectionsStorage.RemoveConnection(clientAddress);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Failed to continue receiving from {clientAddress}: {ex.Message}");
                CleanupConnection(connection, clientAddress);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error continuing receive for {clientAddress}: {e.Message}");
                CleanupConnection(connection, clientAddress);
            }
        }
        else if (connection.Socket != null)
        {
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