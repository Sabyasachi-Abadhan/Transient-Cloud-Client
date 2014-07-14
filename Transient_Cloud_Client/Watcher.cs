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
    /// <summary>
    /// File Watcher service which reads file system events of supported types and produces corresponding custom events onto the queue
    /// </summary>
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
            // We don't currently use this for anything because modify events are triggered by the 
        }

        private void OnCreated(object source, FileSystemEventArgs e)
        {
            if (!FileSystemUtilities.ExtensionIsSupported(e.FullPath) || !FileSystemUtilities.fileIsImportant(e.FullPath))
                return;
            Console.WriteLine("You moved {0} and the deleted file last was {1}", e.Name, nameOfDeletedFile);
            String fileName = FileSystemUtilities.ExtractNameFromPath(e.Name);
            if (fileName.Equals(nameOfDeletedFile))
            {
                Console.WriteLine("You moved {0}", e.Name);
                events.Enqueue(new Event(fileName, pathOfDeletedFile, e.FullPath, FileSystemUtilities.EVENT_ACTIONS.move))  ;
            }
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            if (!FileSystemUtilities.ExtensionIsSupported(e.FullPath) || !FileSystemUtilities.fileIsImportant(e.FullPath))
                return;

            // If any temp file was renamed, send a modify event
            String oldFileName = FileSystemUtilities.ExtractNameFromPath(e.OldName);
            Console.WriteLine(oldFileName);
            if (FileSystemUtilities.isTemporaryFile(oldFileName))
            {
                events.Enqueue(new Event(FileSystemUtilities.ExtractNameFromPath(e.Name), e.FullPath, FileSystemUtilities.EVENT_ACTIONS.modify));
                return;
            }

            // Otherwise, it is a normal rename event so send a put request
            if (FileSystemUtilities.fileIsImportant(e.FullPath))
            {
                Console.WriteLine(FileSystemUtilities.ExtractNameFromPath(e.FullPath) + "|" + e.FullPath);
                events.Enqueue(new Event(FileSystemUtilities.ExtractNameFromPath(e.FullPath), e.OldFullPath, e.FullPath, FileSystemUtilities.EVENT_ACTIONS.rename));
            }
        }

        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            if (!FileSystemUtilities.ExtensionIsSupported(e.FullPath) || !FileSystemUtilities.fileIsImportant(e.FullPath) || FileSystemUtilities.isTemporaryFile(e.FullPath))
                return;
            String fileName = FileSystemUtilities.ExtractNameFromPath(e.Name);
            // This is done to account for move events
            nameOfDeletedFile = fileName;
            pathOfDeletedFile = e.FullPath;
        }

        private void resetLastDeletedFile()
        {
            this.nameOfDeletedFile = "";
            this.pathOfDeletedFile = "";
        }
    }
}