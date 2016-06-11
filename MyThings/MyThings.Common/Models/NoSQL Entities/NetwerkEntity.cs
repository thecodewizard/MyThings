using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MyThings.Common.Models.NoSQL_Entities
{
    #region TableStorageEntities

    public class NetwerkEntity : TableEntity
    {
        public NetwerkEntity() { }
        public NetwerkEntity(string Time, string DevEUI, string FPort, string FCntUp, string ADRbit, string FCntDn,
                                string payload_hex, string mic_hex, string Lrcid, string LrrRSSI, string LrrSNR, string SpFact,
                                string SubBand, string Channel, string DevLrrCnt, string Lrrid, string LrrLAT, string LrrLON, Lrrs lrrs, string CustomerID,
                                CustomerData customerData, string ModelCfg)
        {
            this.PartitionKey = CustomerID;
            this.RowKey = String.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);
            this.Timestamp = DateTime.Now;
            this.Time = Time;
            this.DevEUI = DevEUI;
            this.FPort = FPort;
            this.FCntUp = FCntUp;
            this.ADRbit = ADRbit;
            this.FCntDn = FCntDn;
            this.payload_hex = payload_hex;
            this.mic_hex = mic_hex;
            this.Lrcid = Lrcid;
            this.LrrRSSI = LrrRSSI;
            this.LrrSNR = LrrSNR;
            this.SpFact = SpFact;
            this.SubBand = SubBand;
            this.Channel = Channel;
            this.DevLrrCnt = DevLrrCnt;
            this.Lrrid = Lrrid;
            this.LrrLAT = LrrLAT;
            this.LrrLON = LrrLON;
            this.Lrrid1 = lrrs.Lrr1?.Lrrid;
            this.LrrRSSI1 = lrrs.Lrr1?.LrrRSSI;
            this.LrrSNR1 = lrrs.Lrr1?.LrrSNR;
            this.Lrrid2 = lrrs.Lrr2?.Lrrid;
            this.LrrRSSI2 = lrrs.Lrr2?.LrrRSSI;
            this.LrrSNR2 = lrrs.Lrr2?.LrrSNR;
            this.Lrrid3 = lrrs.Lrr3?.Lrrid;
            this.LrrRSSI3 = lrrs.Lrr3?.LrrRSSI;
            this.LrrSNR3 = lrrs.Lrr3?.LrrSNR;
            this.CustomerID = CustomerID;
            this.pro = customerData?.alr?.pro;
            this.ver = customerData?.alr?.ver;
            this.ModelCfg = ModelCfg;
        }

        public string Time { get; set; }
        public string DevEUI { get; set; }
        public string FPort { get; set; }
        public string FCntUp { get; set; }
        public string ADRbit { get; set; }
        public string FCntDn { get; set; }
        public string payload_hex { get; set; }
        public string mic_hex { get; set; }
        public string Lrcid { get; set; }
        public string LrrRSSI { get; set; }
        public string LrrSNR { get; set; }
        public string SpFact { get; set; }
        public string SubBand { get; set; }
        public string Channel { get; set; }
        public string DevLrrCnt { get; set; }
        public string Lrrid { get; set; }
        public string LrrLAT { get; set; }
        public string LrrLON { get; set; }
        public string Lrrid1 { get; set; }
        public string LrrRSSI1 { get; set; }
        public string LrrSNR1 { get; set; }
        public string Lrrid2 { get; set; }
        public string LrrRSSI2 { get; set; }
        public string LrrSNR2 { get; set; }
        public string Lrrid3 { get; set; }
        public string LrrRSSI3 { get; set; }
        public string LrrSNR3 { get; set; }
        public string CustomerID { get; set; }
        public string pro { get; set; }
        public string ver { get; set; }
        public string ModelCfg { get; set; }
    }
    public class Lrr
    {
        public Lrr(string Lrrid, string LrrRSSI, string LrrSNR)
        {
            this.Lrrid = Lrrid;
            this.LrrRSSI = LrrRSSI;
            this.LrrSNR = LrrSNR;
        }
        public string Lrrid { get; set; }
        public string LrrRSSI { get; set; }
        public string LrrSNR { get; set; }
    }
    public class Lrrs
    {
        public Lrr Lrr1 { get; set; }
        public Lrr Lrr2 { get; set; }
        public Lrr Lrr3 { get; set; }
    }
    public class Alr
    {
        public string pro { get; set; }
        public string ver { get; set; }
    }
    public class CustomerData
    {
        public Alr alr { get; set; }
    }

    #endregion
}
