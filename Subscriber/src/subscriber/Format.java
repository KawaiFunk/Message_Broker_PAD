package subscriber;

public enum Format {
    JSON, XML;

    public static Format fromArg(String s) {
        return (s != null && s.equalsIgnoreCase("xml")) ? XML : JSON;
    }

    public String protocolToken() {
        return this == XML ? "xml" : "json";
    }
}
