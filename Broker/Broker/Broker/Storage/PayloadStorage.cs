using System.Collections.Concurrent;
using Broker.Payloads;

namespace Broker.Storage;

public static class PayloadStorage
{
    private static ConcurrentQueue<Payload> _payloadsQueue;

    static PayloadStorage()
    {
        _payloadsQueue = new  ConcurrentQueue<Payload>();
    }

    public static void AddPayload(Payload payload)
    {
        _payloadsQueue.Enqueue(payload);
    }
    
    public static Payload GetNextPayload()
    {
        Payload payload = null;
        _payloadsQueue.TryDequeue(out payload);
        return payload;
    }
    
    public static bool IsEmpty()
    {
        return _payloadsQueue.IsEmpty;
    }
    
    
}