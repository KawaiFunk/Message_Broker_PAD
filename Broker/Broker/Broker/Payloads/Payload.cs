using System.Xml.Serialization;

namespace Broker.Payloads;

[XmlRoot("Payload")]
public class Payload
{
    [XmlElement("Topic")]
    public string Topic { get; set; } = string.Empty;

    [XmlElement("Message")]
    public string Message { get; set; } = string.Empty;
}