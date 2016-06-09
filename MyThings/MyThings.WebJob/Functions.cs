using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace MyThings.WebJob
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([QueueTrigger("mythingsdecodedqueue")] string message, TextWriter log)
        {
            //Verwerk de inkomende data
                //De json in de message parsen naar een QueueMessageHolder
                //Via QueueMessageHolder de geposte json rij uit tablestorage halen. -> containerentity
                //De bijhorende XML rij ophalen uit de tablestorage. -> ???entity

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



            //TODO: What to do with networkconnectivityError?
            //TODO: When GenericErrors/Warnings -> Do we get errorinfo from proximus?
      

            //Thresholds controleren

            //Virtuele Sensors Berekenen

            //Tabel met time (op einde van de webjob updaten)

            log.WriteLine(message);
        }
    }
}
