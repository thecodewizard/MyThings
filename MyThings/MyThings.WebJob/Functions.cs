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
            //Thresholds controleren

            //Virtuele Sensors Berekenen

            //Tabel met time (op einde van de webjob updaten)

            //Nieuwe sensors (en containers) toevoegen aan sql

            //Count entries update

            log.WriteLine(message);
        }
    }
}
