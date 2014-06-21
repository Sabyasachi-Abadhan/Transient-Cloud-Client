using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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

            // Initialize Shared Queue
            ConcurrentQueue<Event> events = new ConcurrentQueue<Event>();

            // Spawn watcher threads, try to spawn only one thread
            foreach (String directory in directoriesToWatch)
                new Thread(new ThreadStart(new Watcher(directory).watch)).Start();

            // spawn one thread of DocumentMonitor
            DocumentMonitor documentMonitor = new DocumentMonitor(events);
            new Thread(new ThreadStart(documentMonitor.monitor)).Start();

            // spawn one thread of StatisticsCollector
            StatisticsCollector statisticsCollector = new StatisticsCollector(events);
            new Thread(new ThreadStart(statisticsCollector.Collect)).Start();
        }

        private static bool noDirectoriesSpecified()
        {
            return (directoriesToWatch.Length == 0);
        }
    }
}
