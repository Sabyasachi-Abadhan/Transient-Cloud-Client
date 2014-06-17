using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Transient_Cloud_Client
{
    class Watcher
    {
        private static FileSystemWatcher watcher = new FileSystemWatcher();
        public String Path { get; set; }
        private static String extensionsToWatch = "*.*";
        static Watcher()
        {
            watcher.Filter = extensionsToWatch;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.IncludeSubdirectories = true;
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
        }
        public static void Main(String[] args)
        {
            Console.WriteLine("Enter directory to watch");
            //watcher.Path = Console.ReadLine();

            // Start raising events
            //watcher.EnableRaisingEvents = true;
            while (Console.Read() != 'q')
            {
                DocumentMonitor.Print(DocumentMonitor.GetOpenedExcelFiles(new ArrayList()));
                DocumentMonitor.Print(DocumentMonitor.GetOpenedPowerpointFiles(new ArrayList()));
                DocumentMonitor.Print(DocumentMonitor.GetOpenedWordFiles(new ArrayList()));
            }
        }
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }
    }
}