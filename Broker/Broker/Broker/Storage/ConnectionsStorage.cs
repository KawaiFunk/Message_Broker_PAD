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
        if (connection == null) return;

        lock (_locker)
        {
            // Replace existing entry for this address (case-insensitive)
            int idx = _connections.FindIndex(c =>
                string.Equals(c.Address, connection.Address, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0) _connections[idx] = connection;
            else          _connections.Add(connection);
        }
    }

    public static void RemoveConnection(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return;
        lock (_locker)
        {
            _connections.RemoveAll(c =>
                string.Equals(c.Address, address, StringComparison.OrdinalIgnoreCase));
        }
    }
    
    public static List<ConnectionInfo> GetConnectionsByTopic(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic)) return new List<ConnectionInfo>(0);
        lock (_locker)
        {
            return _connections
                .Where(c => !string.IsNullOrWhiteSpace(c.Topic) &&
                            string.Equals(c.Topic, topic, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    public static List<ConnectionInfo> SnapshotAll()
    {
        lock (_locker) { return _connections.ToList(); }
    }

    public static int Count
    {
        get { lock (_locker) { return _connections.Count; } }
    }
    
    
}