using Broker.Networking;

namespace Broker.Storage;

public static class ConnectionsStorage
{
    private static List<ConnectionInfo> _connections;
    private static object               _locker;
    
    static ConnectionsStorage()
    {
        _connections = [];
        _locker      = new object();
    }
    
    public static void AddConnection(ConnectionInfo connection)
    {
        lock (_locker)
        {
            _connections.Add(connection);
        }
    }

    public static void RemoveConnection(string address)
    {
        lock (_locker)
        {
            _connections.RemoveAll(it => it.Address == address);
        }
    }
    
    public static List<ConnectionInfo> GetConnectionsByTopic(string topic)
    {
        lock (_locker)
        {
            return _connections.Where(it => it.Topic == topic).ToList();
        }
    }
    
    
}