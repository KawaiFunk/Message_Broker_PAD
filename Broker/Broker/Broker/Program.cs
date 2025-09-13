using Broker;
using Broker.Networking;

Console.WriteLine("Broker is starting...");

var brokerSocket = new BrokerSocket();
brokerSocket.Start(Constants.BrokerIp, Constants.BrokerPort);

var worker = new Worker();
Task.Factory.StartNew(worker.DoSendMessageWork, TaskCreationOptions.LongRunning);

Console.WriteLine("Press ENTER to exit.");
Console.ReadLine();