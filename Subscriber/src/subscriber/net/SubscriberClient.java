package subscriber.net;

import subscriber.Format;
import subscriber.handler.MessageHandler;
import subscriber.util.IoUtil;

import java.io.*;
import java.net.*;
import java.nio.charset.StandardCharsets;

public class SubscriberClient {
    private final String host;
    private final int    port;
    private final String topic;
    private final Format format;
    private final MessageHandler handler;

    public SubscriberClient(String host, int port, String topic, Format format, MessageHandler handler) {
        this.host = host;
        this.port = port;
        this.topic = topic;
        this.format = format;
        this.handler = handler;
    }

    public void run() throws IOException {
        try (Socket sock = new Socket()) {
            // Connect + basic socket tuning
            sock.connect(new InetSocketAddress(host, port), 3000);
            sock.setTcpNoDelay(true);
            try { sock.setKeepAlive(true); } catch (Exception ignore) {}

            try (BufferedWriter out = new BufferedWriter(
                        new OutputStreamWriter(sock.getOutputStream(), StandardCharsets.UTF_8));
                 BufferedReader in  = new BufferedReader(
                        new InputStreamReader(sock.getInputStream(), StandardCharsets.UTF_8))) {

                // 1) Set connection format
                IoUtil.sendLine(out, "FORMAT:" + format.protocolToken());
                // 2) Subscribe to topic
                IoUtil.sendLine(out, "SUBSCRIBE:" + topic);

                System.out.printf("Connected to %s:%d, subscribed to \"%s\" (fmt=%s)%n",
                        host, port, topic, format);

                // 3) Read frames forever (one message per line)
                String line;
                while ((line = in.readLine()) != null) {
                    if (line.isBlank()) continue;
                    if (line.equalsIgnoreCase("PONG")) continue; // ignore heartbeats
                    handler.onLine(line);
                }
                System.out.println("Server closed the connection.");
            }
        }
    }
}
