using System;
using System.IO;
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

            Initialize();

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

        private static void Initialize()
        {
            // load settings from configuration file
            directoriesToWatch = System.Configuration.ConfigurationManager.AppSettings["DirectoriesToWatch"].Split(',');
            url = System.Configuration.ConfigurationManager.AppSettings["ApiUrl"];
            if (NoDirectoriesSpecified())
                directoriesToWatch = System.Configuration.ConfigurationManager.AppSettings["DefaultDirectoriesToWatch"].Split(',');

            // Dropbox Folder Initializations
            String dropboxDirectory = System.Configuration.ConfigurationManager.AppSettings["DropboxDirectory"];
            String transientCloudDirectory = System.Configuration.ConfigurationManager.AppSettings["TransientCloudDirectoryName"];
            String transientCloudPath = String.Concat(dropboxDirectory, transientCloudDirectory);
            if (!Directory.Exists(transientCloudPath))
                Directory.CreateDirectory(transientCloudPath);
        }

        private static bool NoDirectoriesSpecified()
        {
            return (directoriesToWatch.Length == 0);
        }
    }
}
