package subscriber.handler;
import subscriber.util.IoUtil;  

public class PrintingMessageHandler implements MessageHandler {
    @Override
    public void onLine(String line) {
        char c = IoUtil.firstNonSpace(line);
        if (c == '{') {
            System.out.println("JSON message: " + line);
        } else if (c == '<') {
            System.out.println("XML message: " + line);
        } else {
            System.out.println("Server: " + line);
        }
    }
}