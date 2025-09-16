package subscriber;

public record CliOptions(String host, int port, String topic, Format format) {
    public static CliOptions parse(String[] args) {
        if (args.length < 4) {
            System.out.println("Usage: java com.example.subscriber.App <host> <port> <topic> <json|xml>");
            System.out.println("Example: java com.example.subscriber.App 127.0.0.1 8080 logs/app json");
            return null;
        }
        String host  = args[0];
        int    port  = Integer.parseInt(args[1]);
        String topic = args[2];
        Format fmt   = Format.fromArg(args[3]);
        return new CliOptions(host, port, topic, fmt);
    }
}
