namespace Broker;

public static class Constants
{
    public static readonly int    ConnectionQueueSize = 20;
    public static readonly int    BufferSize          = 1024;
    public static readonly string BrokerIp            = "0.0.0.0";
    public static readonly int    BrokerPort          = 8080;
    public static readonly int    WorkerSleepTimeSec  = 1;
}