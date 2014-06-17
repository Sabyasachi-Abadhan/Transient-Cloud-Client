using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Transient_Cloud_Client
{
    class Application
    {
        private static String[] directoriesToWatch;
        private static String url;

        public static void Main(String[] arguments)
        {
            // load settings from configuration file
            directoriesToWatch = System.Configuration.ConfigurationManager.AppSettings["DirectoriesToWatch"].Split(',');
            url = System.Configuration.ConfigurationManager.AppSettings["ApiUrl"];
            if (noDirectoriesSpecified())
                directoriesToWatch = System.Configuration.ConfigurationManager.AppSettings["DefaultDirectoriesToWatch"].Split(',');

            // Spawn watcher threads
            foreach (String directory in directoriesToWatch)
                new Thread(new ThreadStart(new Watcher(directory).watch)).Start();

            // spawn one thread of DocumentMonitor
            new Thread(new ThreadStart(DocumentMonitor.monitor)).Start();
            
            // spawn one thread of StatisticsCollector
        }

        private static bool noDirectoriesSpecified()
        {
            return (directoriesToWatch.Length == 0);
        }
    }
}
