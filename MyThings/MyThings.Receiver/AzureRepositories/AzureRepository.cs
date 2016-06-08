using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Proximus_API.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;


namespace Proximus_Webservice.Repositories
{
    public class AzureRepository
    {
        #region Blob
        public static void WriteToBlobNetwerk(string data)
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("proximusnetwerk");
            container.CreateIfNotExists();
            // Retrieve reference to a blob.
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(DateTime.Now.Ticks.ToString() + ".txt");
            // Create or overwrite the blob with contents from a local file.
            blockBlob.UploadText(data);
        }

        public static void WriteToBlobDecoded(string data)
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("proximusdecoded");
            container.CreateIfNotExists();
            // Retrieve reference to a blob.
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(DateTime.Now.Ticks.ToString() + ".txt");
            // Create or overwrite the blob with contents from a local file.
            blockBlob.UploadText(data);
        }
        #endregion
        #region Table
        public static void WriteToTableNetwerk(DevEUI_uplink data)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            // Create the CloudTable object that represents the "temperature" table.
            CloudTable table = tableClient.GetTableReference("proximusnetwerktable");
            table.CreateIfNotExists();
            // Make the object

            Lrrs lijst = new Lrrs();
            int amount = data.Lrrs.Lrr.Count;
            if (amount > 0) lijst.Lrr1 = new Lrr(data.Lrrs.Lrr[0].Lrrid, data.Lrrs.Lrr[0].LrrRSSI, data.Lrrs.Lrr[0].LrrSNR);
            if (amount > 1) lijst.Lrr2 = new Lrr(data.Lrrs.Lrr[1].Lrrid, data.Lrrs.Lrr[1].LrrRSSI, data.Lrrs.Lrr[1].LrrSNR);
            if (amount > 2) lijst.Lrr3 = new Lrr(data.Lrrs.Lrr[2].Lrrid, data.Lrrs.Lrr[2].LrrRSSI, data.Lrrs.Lrr[2].LrrSNR);

            CustomerData customerData = JsonConvert.DeserializeObject<CustomerData>(data.CustomerData);

            NetwerkEntity entity = new NetwerkEntity(data.Time, data.DevEUI, data.FPort, data.FCntUp, data.ADRbit, data.FCntDn, 
                                                     data.Payload_hex, data.Mic_hex, data.Lrcid, data.LrrRSSI, data.LrrSNR, data.SpFact, data.SubBand,
                                                     data.Channel, data.DevLrrCnt, data.Lrrid, lijst, data.CustomerID, customerData, data.ModelCfg);
            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(entity);
            // Execute the insert operation.
            table.Execute(insertOperation);
        }
        public static void WriteToTableDecoded(DecodedData data)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            // Create the CloudTable object that represents the "temperature" table.
            CloudTable table = tableClient.GetTableReference("proximusdecodedtable");
            table.CreateIfNotExists();
            // Make the entity
            DecodedEntity entity = new DecodedEntity(data.company, data.macaddress, data.container, data.locationid, data.payload, data.timestamp);
            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(entity);
            // Execute the insert operation.
            table.Execute(insertOperation);
            PutOnStorageQueue(entity.PartitionKey, entity.RowKey);
        }
        #endregion
        #region entities

        class ErrorEntity : TableEntity
        {
            public ErrorEntity(Exception ex)
            {
                this.PartitionKey = ex.Message + " - " + ex.StackTrace;
                this.RowKey = DateTime.Now.Ticks.ToString();
                this.Timestamp = DateTime.Now;
            }

            public ErrorEntity() { }
        }
        class DecodedEntity : TableEntity
        {
            public DecodedEntity(string company, string macaddress, string container, string locationid, string payload, string timestamp)
            {
                this.PartitionKey = macaddress;
                this.RowKey = String.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);
                this.company = company;
                this.macaddress = macaddress;
                this.container = container;
                this.locationid = locationid;
                this.payload = double.Parse(payload, CultureInfo.InvariantCulture);
                this.receivedtimestamp = timestamp;
                this.Timestamp = DateTime.Now;
            }

            public DecodedEntity() { }
            public string company { get; set; }
            public string macaddress { get; set; }
            public string container { get; set; }
            public string locationid { get; set; }
            public double payload { get; set; }
            public string receivedtimestamp { get; set; }

        }
        class NetwerkEntity : TableEntity
        {
            public NetwerkEntity(string Time, string DevEUI, string FPort, string FCntUp, string ADRbit, string FCntDn,
                                string payload_hex, string mic_hex, string Lrcid, string LrrRSSI, string LrrSNR, string SpFact,
                                string SubBand, string Channel, string DevLrrCnt, string Lrrid, Lrrs lrrs, string CustomerID,
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
                this.Lrrid1 = lrrs.Lrr1.Lrrid;
                this.LrrRSSI1 = lrrs.Lrr1.LrrRSSI;
                this.LrrSNR1 = lrrs.Lrr1.LrrSNR;
                this.Lrrid2 = lrrs.Lrr2.Lrrid;
                this.LrrRSSI2 = lrrs.Lrr2.LrrRSSI;
                this.LrrSNR2 = lrrs.Lrr2.LrrSNR;
                this.Lrrid3 = lrrs.Lrr3.Lrrid;
                this.LrrRSSI3 = lrrs.Lrr3.LrrRSSI;
                this.LrrSNR3 = lrrs.Lrr3.LrrSNR;
                this.CustomerID = CustomerID;
                this.pro = customerData.alr.pro;
                this.ver = customerData.alr.ver;
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
        class Lrr
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
        class Lrrs
        {
            public Lrr Lrr1 { get; set; }
            public Lrr Lrr2 { get; set; }
            public Lrr Lrr3 { get; set; }
        }
        class Alr
        {
            public string pro { get; set; }
            public string ver { get; set; }
        }
        class CustomerData
        {
            public Alr alr { get; set; }
        }

        #endregion
        #region error
        public static void WriteErrorToTable(Exception ex)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            // Create the CloudTable object that represents the "temperature" table.
            CloudTable table = tableClient.GetTableReference("proximuserrorlog");
            table.CreateIfNotExists();
            // Make the entity
            ErrorEntity entity = new ErrorEntity(ex);
            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(entity);
            // Execute the insert operation.
            table.Execute(insertOperation);
        }
        #endregion
        #region service bus queue

        public static void PutOnStorageQueue(String partitionkey, String rowkey)
        {
            // Make the object that will carry the queuemessage
            QueueObject qo = new QueueObject(partitionkey, rowkey);
            String json = JsonConvert.SerializeObject(qo);

            // Create the queue if it does not exist already.
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.QueueExists("mythingsdecodedqueue"))
            {
                namespaceManager.CreateQueue("mythingsdecodedqueue");
            }

            //Send The Queue Message
            QueueClient Client = QueueClient.CreateFromConnectionString(connectionString, "mythingsdecodedqueue");
            BrokeredMessage message = new BrokeredMessage(json);
            Client.Send(message);
        }

        public class QueueObject
        {
            public String PartitionKey { get; set; }
            public String RowKey { get; set; }

            public QueueObject(string partitionKey, string rowKey)
            {
                PartitionKey = partitionKey;
                RowKey = rowKey;
            }
        }

        #endregion
    }
}
