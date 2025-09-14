using Broker.Payloads.Enums;

namespace Broker.Payloads;

public static class MessageFormatManager
{
    private static MessageFormat _currentFormat = MessageFormat.Json;
    private static readonly object _lock = new object();

    public static MessageFormat CurrentFormat
    {
        get
        {
            lock (_lock)
            {
                return _currentFormat;
            }
        }
        set
        {
            lock (_lock)
            {
                _currentFormat = value;
            }
        }
    }
    
    public static void SwitchFormat()
    {
        lock (_lock)
        {
            _currentFormat = _currentFormat == MessageFormat.Json ? MessageFormat.Xml : MessageFormat.Json;
        }
    }
}