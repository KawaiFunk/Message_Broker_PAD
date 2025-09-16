package subscriber;

import subscriber.handler.PrintingMessageHandler;
import subscriber.net.SubscriberClient;

public class App {
   public App() {
   }

   public static void main(String[] var0) {
      CliOptions var1 = CliOptions.parse(var0);
      if (var1 != null) {
         PrintingMessageHandler var2 = new PrintingMessageHandler();
         SubscriberClient var3 = new SubscriberClient(var1.host(), var1.port(), var1.topic(), var1.format(), var2);

         try {
            var3.run();
         } catch (Exception var5) {
            System.err.println("Subscriber error: " + var5.getMessage());
         }

      }
   }
}
