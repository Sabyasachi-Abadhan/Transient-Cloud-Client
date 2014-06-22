using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Security.Cryptography;
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
                case Utilities.EVENT_ACTIONS.open:
                    CreateHandler(currentEvent);
                    break;
                case Utilities.EVENT_ACTIONS.delete:
                    DeleteHandler(currentEvent);
                    break;
                case Utilities.EVENT_ACTIONS.rename:
                    if (IsTracked(currentEvent.File.Name))
                        fileList[currentEvent.File.Name] = new File(currentEvent.File.Name, currentEvent.File.Path);
                    break;
            
            }
        }

        private void CreateHandler(Event currentEvent)
        {
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
        }

        private void DeleteHandler(Event currentEvent)
        {
            if (IsTracked(currentEvent.File.Name))
            {
                // delete from transient folder
                DeleteFile(currentEvent.File);

                // delete from the server
                sendDeleteRequest(currentEvent.File);

                // remove from internal data structure
                fileList.Remove(currentEvent.File.Name);
            }
        }

        private static void DeleteFile(File file)
        {
            SystemFile.Delete(String.Concat(Settings.transientCloudDirectoryPath, file.Name));
        }
        
        private static void CopyFile(File file)
        {
            Console.WriteLine("Currently Processing: " + file.Name);
            String destination = String.Concat(Settings.transientCloudDirectoryPath, file.Name);
            String source = file.Path;
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
                    Console.WriteLine(source + "|" + destination);
                }
        }

        private void PostToServer(File file)
        {
            // 
        }

        private void sendDeleteRequest(File file)
        {
            // Send the name and hash of file so the server can delete it
        }
        private int generateMD5Hash(File file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = SystemFile.OpenRead(file.Path))
                {
                    Byte[] bytes = md5.ComputeHash(stream);
                    return BitConverter.ToInt32(bytes, 0);
                }
            }
        }
        
        public Boolean IsTracked(String fileName)
        {
            return fileList.ContainsKey(fileName);
        }
    }
}
