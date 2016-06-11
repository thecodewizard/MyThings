using System.Collections.Generic;

namespace MyThings.Common.Models
{
    public class Lrr
    {
        public string Lrrid { get; set; }
        public string LrrRSSI { get; set; }
        public string LrrSNR { get; set; }
    }

    public class Lrrs
    {
        public List<Lrr> Lrr { get; set; }
    }

    public class LocationWebhookModel
    {
        public string Time { get; set; }
        public string DevEUI { get; set; }
        public string FPort { get; set; }
        public string FCntUp { get; set; }
        public string ADRbit { get; set; }
        public string FCntDn { get; set; }
        public string Payload_hex { get; set; }
        public string Mic_hex { get; set; }
        public string Lrcid { get; set; }
        public string LrrRSSI { get; set; }
        public string LrrSNR { get; set; }
        public string SpFact { get; set; }
        public string SubBand { get; set; }
        public string Channel { get; set; }
        public string DevLrrCnt { get; set; }
        public string Lrrid { get; set; }
        public Lrrs Lrrs { get; set; }
        public string CustomerID { get; set; }
        public string CustomerData { get; set; }
        public string ModelCfg { get; set; }
        public object Xmlns { get; set; }
        public int LrrLAT { get; set; }
        public int LrrLON { get; set; }
        public string LrrsString { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public int accuracy { get; set; }
        public double averageCount { get; set; }
        public double averageLat { get; set; }
        public double averageLong { get; set; }
    }
}
