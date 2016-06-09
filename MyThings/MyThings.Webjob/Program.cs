using Microsoft.Azure;
using Newtonsoft.Json;

namespace MyThings.WebjobSample
{
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using System;
    using System.Threading.Tasks;

    public class Program
    {
        public static void Main(string[] args)
        {
            //Get the Queue
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the queue client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a container.
            CloudQueue queue = queueClient.GetQueueReference("mythingsdecodedqueue");

            if (queue.Exists())
            {
                // Peek at the next message
                CloudQueueMessage peekedMessage = queue.PeekMessage();

                // Display message.
                Console.WriteLine(peekedMessage.AsString);

                // Fetch the queue attributes.
                queue.FetchAttributes();

                // Retrieve the cached approximate message count.
                int? cachedMessageCount = queue.ApproximateMessageCount;

                // Display number of messages.
                Console.WriteLine("Number of messages in queue: " + cachedMessageCount);

                // Get the next message
                CloudQueueMessage retrievedMessage = queue.GetMessage();

                //Process the message in less than 30 seconds, and then delete the message
                queue.DeleteMessage(retrievedMessage);
            }

            //Do the onCompletedWork

            //Standard Console Logic
            Console.WriteLine("Press any key to exit");
            Console.Read();
        }

        static void RunOnceInWebjob()
        {
            //Error-Warning Module
                //Check voor voorspelde connectivity errors
                    //Bereken op basis van de sensorentries en de creationdate een gemiddelde responstijd van de sensor.
                    //Controleer ofdat de laatste waarde die gekend is van de sensor deze responstijd
                    //-> Indien 2x langer: toon networkconnectivity error.

            //Virtuele Sensors Berekenen
                //

            //Timelogging
                //Schrijf naar SQL wanneer deze webjob voor het laatst ge-execute heeft
        }

        static void RunPerSensor()
        {
            //Verwerk de inkomende data
                //De json in de message parsen naar een QueueMessageHolder
                //Via QueueMessageHolder de geposte json rij uit tablestorage halen. -> containerentity

            //Database Population Management
                //Check in SQL of het containertype bestaat.
                    //-> Nee: Maak een nieuw containertype aan en voeg toe

                //Check in sql of de sensor met het MAC Address al bestaat
                    //-> Nee: Maak een nieuwe aan met sensorentries '1'
                    //-> Ja: Haal op en tel '1' bij op het sensorentries veld

                //Check of de container aanwezig is in de containerlijst van de sensor
                    //-> Nee: Maak een nieuwe container aan (wijs juiste type en containertype toe) en voeg toe
                    //-> Ja: Haal op

            //Error-Warning Module
                //Check voor de huidige container of de waarde overschreden is.
                    //-> Ja: Resulteerd in MinThresholdWarning of MaxThresholdWarning
                    //-> Indien containertype battery:
                //Bereken adhv de containerhistory wanneer de battery zal leeg zijn.
                    //-> Onder 15%: Geef BatteryWarning
                    //-> Onder 5%: Geef BatteryCriticalError
                //Check voor de andere containers van de sensor van wanneer hun meest recente waarde is.
                    //-> Langer dan 1 week: InactiveContainerWarning
                    //-> Indien alle containers van een sensor langer dan 1 week niets gestuurd hebben -> InactiveSensorWarning
        }
    }
}
