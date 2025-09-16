package subscriber.util;

import java.io.BufferedWriter;
import java.io.IOException;

public final class IoUtil {
    private IoUtil() {}

    public static void sendLine(BufferedWriter out, String line) throws IOException {
        out.write(line);
        out.write('\n');
        out.flush();
    }

    public static char firstNonSpace(String s) {
        for (int i = 0; i < s.length(); i++) {
            char c = s.charAt(i);
            if (!Character.isWhitespace(c)) return c;
        }
        return '\0';
    }
}