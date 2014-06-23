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

        public static void Main(String[] arguments)
        {

            Initialize();

            // Initialize Shared Queue
            ConcurrentQueue<Event> events = new ConcurrentQueue<Event>();

            // Spawn watcher threads, try to spawn only one thread

            foreach (String directory in Settings.directoriesToWatch)
                new Thread(new ThreadStart(new Watcher(directory, events).watch)).Start();

            // spawn one thread of DocumentMonitor
            DocumentMonitor documentMonitor = new DocumentMonitor(ref events);
            new Thread(new ThreadStart(documentMonitor.monitor)).Start();

            // spawn one thread of StatisticsCollector
            StatisticsCollector statisticsCollector = new StatisticsCollector(ref events);
            new Thread(new ThreadStart(statisticsCollector.Collect)).Start();
        }

        private static void Initialize()
        {
            // load settings from configuration file
            Settings.directoriesToWatch = System.Configuration.ConfigurationManager.AppSettings["DirectoriesToWatch"].Split(',');
            Settings.apiUrl = System.Configuration.ConfigurationManager.AppSettings["ApiUrl"];
            Settings.defaultDirectoriesToWatch = System.Configuration.ConfigurationManager.AppSettings["DefaultDirectoriesToWatch"].Split(',');
            if (NoDirectoriesSpecified())
                Settings.directoriesToWatch = Settings.defaultDirectoriesToWatch;

            // Dropbox Folder Initializations
            Settings.dropboxDirectory = System.Configuration.ConfigurationManager.AppSettings["DropboxDirectory"];
            Settings.transientCloudDirectoryName = Environment.UserName;
            Settings.transientCloudDirectoryPath = String.Concat(Settings.dropboxDirectory, Settings.transientCloudDirectoryName, "\\");
            if (!Directory.Exists(Settings.transientCloudDirectoryPath))
                Directory.CreateDirectory(Settings.transientCloudDirectoryPath);

            //Check if Web Server is Online
            Console.WriteLine(Utilities.IsServerOnline() ? "Server is Online" : "Server is Offline");
        }

        private static bool NoDirectoriesSpecified()
        {
            return (Settings.directoriesToWatch.Length == 0);
        }
    }
}
