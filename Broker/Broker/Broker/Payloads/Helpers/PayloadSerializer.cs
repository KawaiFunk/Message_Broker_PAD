using System.Xml;
using System.Xml.Serialization;
using Broker.Payloads.Enums;
using Newtonsoft.Json;

namespace Broker.Payloads.Helpers;

public class PayloadSerializer
{
    public static string Serialize(Payload payload, MessageFormat format)
    {
        return format switch
               {
                   MessageFormat.Json => JsonConvert.SerializeObject(payload),
                   MessageFormat.Xml => SerializeToXml(payload),
                   _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
               };
    }

    public static Payload Deserialize(string data, MessageFormat format)
    {
        return format switch
               {
                   MessageFormat.Json => JsonConvert.DeserializeObject<Payload>(data),
                   MessageFormat.Xml => DeserializeFromXml(data),
                   _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
               };
    }

    private static string SerializeToXml(Payload payload)
    {
        var       xmlSerializer = new XmlSerializer(typeof(Payload));
        using var stringWriter  = new StringWriter();
        using var xmlWriter     = XmlWriter.Create(stringWriter);
        xmlSerializer.Serialize(xmlWriter, payload);
        return stringWriter.ToString();
    }

    private static Payload DeserializeFromXml(string xml)
    {
        var       xmlSerializer = new XmlSerializer(typeof(Payload));
        using var stringReader  = new StringReader(xml);
        return (Payload)xmlSerializer.Deserialize(stringReader);
    }
}