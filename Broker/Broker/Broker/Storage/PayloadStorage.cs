using System.Collections.Concurrent;
using Broker.Payloads;

namespace Broker.Storage;

public static class PayloadStorage
{
    private static readonly BlockingCollection<Payload> _queue =
        new(new ConcurrentQueue<Payload>());

    public static void AddPayload(Payload payload) => _queue.Add(payload);

    public static IEnumerable<Payload> GetConsumingEnumerable() => _queue.GetConsumingEnumerable();

    public static void Complete() => _queue.CompleteAdding();
    
}