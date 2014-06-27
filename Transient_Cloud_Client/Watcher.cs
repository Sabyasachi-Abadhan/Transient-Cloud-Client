using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace Transient_Cloud_Client
{
    class Watcher
    {
        private FileSystemWatcher watcher;
        private String extensionsToWatch = "*.*";
        private ConcurrentQueue<Event> events;
        private String nameOfDeletedFile = "";
        private String pathOfDeletedFile = "";

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
            watcher.Created += new FileSystemEventHandler(OnCreated);
            watcher.Deleted += new FileSystemEventHandler(OnDeleted);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
            watcher.EnableRaisingEvents = true;
            Console.WriteLine("Starting to watch: " + this.watcher.Path);
            while (true) ;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (!FileSystemUtilities.ExtensionIsSupported(e.FullPath))
                return;
            // Hack to ensure that the file have been modified completely
            Console.WriteLine("You Edited {0}", e.Name);
            Thread.Sleep(5000);
            events.Enqueue(new Event(FileSystemUtilities.ExtractNameFromPath(e.Name), e.FullPath, FileSystemUtilities.EVENT_ACTIONS.modify));
        }

        private void OnCreated(object source, FileSystemEventArgs e)
        {
            if (!FileSystemUtilities.ExtensionIsSupported(e.FullPath))
                return;
            Console.WriteLine("You moved {0} and the deleted file last was {1}", e.Name, nameOfDeletedFile);
            String fileName = FileSystemUtilities.ExtractNameFromPath(e.Name);
            if (FileSystemUtilities.ExtractNameFromPath(e.Name).Equals(nameOfDeletedFile))
            {
                Console.WriteLine("You moved {0}", e.Name);
                events.Enqueue(new Event(fileName, pathOfDeletedFile, e.FullPath, FileSystemUtilities.EVENT_ACTIONS.move));
            }
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            // If the new path is outside one of the watched directories, we can go ahead and delete 
            if (!FileSystemUtilities.ExtensionIsSupported(e.FullPath))
                return;
            events.Enqueue(new Event(FileSystemUtilities.ExtractNameFromPath(e.OldFullPath), e.OldFullPath, FileSystemUtilities.EVENT_ACTIONS.delete));
            if (FileSystemUtilities.fileIsImportant(e.FullPath))
            {
                Console.WriteLine(FileSystemUtilities.ExtractNameFromPath(e.FullPath) + "|" + e.FullPath);
                events.Enqueue(new Event(FileSystemUtilities.ExtractNameFromPath(e.FullPath), e.FullPath, FileSystemUtilities.EVENT_ACTIONS.open));
            }
        }

        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            if (!FileSystemUtilities.ExtensionIsSupported(e.FullPath))
                return;
            String fileName = FileSystemUtilities.ExtractNameFromPath(e.Name);
            if (!e.FullPath.Contains(Settings.transientCloudDirectoryPath))
            {
                Console.WriteLine("Enqueing delete operation on file {0}", fileName);
                // This is done to account for move events
                nameOfDeletedFile = fileName;
                pathOfDeletedFile = e.FullPath;
                events.Enqueue(new Event(fileName, e.FullPath, FileSystemUtilities.EVENT_ACTIONS.delete));
            }
        }

        private void resetLastDeletedFile()
        {
            this.nameOfDeletedFile = "";
            this.pathOfDeletedFile = "";
        }
    }
}