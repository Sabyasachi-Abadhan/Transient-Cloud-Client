using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;

namespace Transient_Cloud_Client
{
    class Watcher
    {
        private FileSystemWatcher watcher;
        private String extensionsToWatch = "*.*";
        private ConcurrentQueue<Event> events;
        public Watcher(String directoryToWatch, ConcurrentQueue<Event> events)
        {
            watcher = new FileSystemWatcher();
            watcher.Path = directoryToWatch;
            this.events = events;
        }
        public void watch()
        {
            watcher.Filter = extensionsToWatch;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.IncludeSubdirectories = true;
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnDeleted);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
            watcher.EnableRaisingEvents = true;
            Console.WriteLine("Starting to watch: " + this.watcher.Path);
            while (true) ;
        }
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            // Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            // If the new path is outside one of the watched directories, we can go ahead and delete 
            events.Enqueue(new Event(extractNameFromPath(e.OldFullPath), e.OldFullPath, Utilities.EVENT_ACTIONS.delete));
            if (Utilities.fileIsImportant(e.FullPath))
            {
                Console.WriteLine(extractNameFromPath(e.FullPath) + "|" + e.FullPath);
                events.Enqueue(new Event(extractNameFromPath(e.FullPath), e.FullPath, Utilities.EVENT_ACTIONS.open));
            }
        }

        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            String fileName = extractNameFromPath(e.Name);
            if (!e.FullPath.Contains(Settings.transientCloudDirectoryPath))
            {
                Console.WriteLine("Enqueing delete operation on file {0}", fileName);
                events.Enqueue(new Event(fileName, e.FullPath, Utilities.EVENT_ACTIONS.delete));
            }
        }

        private String extractNameFromPath(String path)
        {
            return path.Substring(path.LastIndexOf("\\") + 1);
        }
    }
}