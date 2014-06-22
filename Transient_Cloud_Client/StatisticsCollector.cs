using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using SystemFile = System.IO.File;
using System.Linq;
using System.Text;

namespace Transient_Cloud_Client
{
    class StatisticsCollector
    {
        private ConcurrentQueue<Event> events;
        private Dictionary<String, File> fileList = new Dictionary<String, File>();
        public StatisticsCollector(ref ConcurrentQueue<Event> events)
        {
            this.events = events;
        }
        public void Collect()
        {
            Event current;
            while (true)
            {
                events.TryDequeue(out current);
                if (current != null)
                    ProcessEvent(current);
            }
        }

        private void ProcessEvent(Event currentEvent)
        {
            switch (currentEvent.Action)
            {
                case "open":
                    if (!IsTracked(currentEvent.File.Name))
                    {
                        fileList.Add(currentEvent.File.Name, currentEvent.File);
                        CopyFile(currentEvent.File);
                    }

                    // Have to decide what to here:
                    else
                    {
                        fileList[currentEvent.File.Name].UpdateLastModified();
                        CopyFile(currentEvent.File);
                    }
                    break;
            }
        }

        private static void CopyFile(File file)
        {
            Console.WriteLine("Currently Processing: " + file.Name);
            String destination = String.Concat(Settings.transientCloudDirectoryPath, file.Name);
            String source = String.Concat(file.Path, "\\", file.Name);
            // Does destination file exist?
            // Don't copy if the last modified date of the file is the same as what is present in the directory

            if(!SystemFile.Exists(destination) || SystemFile.GetLastWriteTime(destination) <= SystemFile.GetLastWriteTime(source))
                try
                {
                    System.IO.File.Copy(source, destination, true);
                }
                catch
                {
                    Console.WriteLine("Couldn't copy at this time");
                }
        }

        public Boolean IsTracked(String fileName)
        {
            return fileList.ContainsKey(fileName);
        }
    }
}
