using System.Xml.Serialization;
using System.Collections.Generic;

namespace Proximus_API.Models
{
    [XmlRoot(ElementName = "Lrr", Namespace = "http://uri.actility.com/lora")]
    public class Lrr
    {
        [XmlElement(ElementName = "Lrrid", Namespace = "http://uri.actility.com/lora")]
        public string Lrrid { get; set; }
        [XmlElement(ElementName = "LrrRSSI", Namespace = "http://uri.actility.com/lora")]
        public string LrrRSSI { get; set; }
        [XmlElement(ElementName = "LrrSNR", Namespace = "http://uri.actility.com/lora")]
        public string LrrSNR { get; set; }
    }

    [XmlRoot(ElementName = "Lrrs", Namespace = "http://uri.actility.com/lora")]
    public class Lrrs
    {
        [XmlElement(ElementName = "Lrr", Namespace = "http://uri.actility.com/lora")]
        public List<Lrr> Lrr { get; set; }
    }

    [XmlRoot(ElementName = "DevEUI_uplink", Namespace = "http://uri.actility.com/lora")]
    public class DevEUI_uplink
    {
        [XmlElement(ElementName = "Time", Namespace = "http://uri.actility.com/lora")]
        public string Time { get; set; }
        [XmlElement(ElementName = "DevEUI", Namespace = "http://uri.actility.com/lora")]
        public string DevEUI { get; set; }
        [XmlElement(ElementName = "FPort", Namespace = "http://uri.actility.com/lora")]
        public string FPort { get; set; }
        [XmlElement(ElementName = "FCntUp", Namespace = "http://uri.actility.com/lora")]
        public string FCntUp { get; set; }
        [XmlElement(ElementName = "ADRbit", Namespace = "http://uri.actility.com/lora")]
        public string ADRbit { get; set; }
        [XmlElement(ElementName = "FCntDn", Namespace = "http://uri.actility.com/lora")]
        public string FCntDn { get; set; }
        [XmlElement(ElementName = "payload_hex", Namespace = "http://uri.actility.com/lora")]
        public string Payload_hex { get; set; }
        [XmlElement(ElementName = "mic_hex", Namespace = "http://uri.actility.com/lora")]
        public string Mic_hex { get; set; }
        [XmlElement(ElementName = "Lrcid", Namespace = "http://uri.actility.com/lora")]
        public string Lrcid { get; set; }
        [XmlElement(ElementName = "LrrRSSI", Namespace = "http://uri.actility.com/lora")]
        public string LrrRSSI { get; set; }
        [XmlElement(ElementName = "LrrSNR", Namespace = "http://uri.actility.com/lora")]
        public string LrrSNR { get; set; }
        [XmlElement(ElementName = "SpFact", Namespace = "http://uri.actility.com/lora")]
        public string SpFact { get; set; }
        [XmlElement(ElementName = "SubBand", Namespace = "http://uri.actility.com/lora")]
        public string SubBand { get; set; }
        [XmlElement(ElementName = "Channel", Namespace = "http://uri.actility.com/lora")]
        public string Channel { get; set; }
        [XmlElement(ElementName = "DevLrrCnt", Namespace = "http://uri.actility.com/lora")]
        public string DevLrrCnt { get; set; }
        [XmlElement(ElementName = "Lrrid", Namespace = "http://uri.actility.com/lora")]
        public string Lrrid { get; set; }
        [XmlElement(ElementName = "LrrLAT", Namespace = "http://uri.actility.com/lora")]
        public string LrrLAT { get; set; }
        [XmlElement(ElementName = "LrrLON", Namespace = "http://uri.actility.com/lora")]
        public string LrrLON { get; set; }
        [XmlElement(ElementName = "Lrrs", Namespace = "http://uri.actility.com/lora")]
        public Lrrs Lrrs { get; set; }
        [XmlElement(ElementName = "CustomerID", Namespace = "http://uri.actility.com/lora")]
        public string CustomerID { get; set; }
        [XmlElement(ElementName = "CustomerData", Namespace = "http://uri.actility.com/lora")]
        public string CustomerData { get; set; }
        [XmlElement(ElementName = "ModelCfg", Namespace = "http://uri.actility.com/lora")]
        public string ModelCfg { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }

}
