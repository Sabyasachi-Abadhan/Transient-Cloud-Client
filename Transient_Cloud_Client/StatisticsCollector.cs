using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using SystemFile = System.IO.File;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;

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
                    OpenHandler(currentEvent);
                    break;
                case Utilities.EVENT_ACTIONS.delete:
                    DeleteHandler(currentEvent);
                    break;
                case Utilities.EVENT_ACTIONS.rename:
                    if (IsTracked(currentEvent.File.Name))
                        fileList[currentEvent.File.Name] = new File(currentEvent.File.Name, currentEvent.File.Path);
                    break;
                case Utilities.EVENT_ACTIONS.move:
                    MoveHandler(currentEvent);
                    break;
                case Utilities.EVENT_ACTIONS.modify:
                    ModifyHandler(currentEvent);
                    break;
            }
        }

        private void MoveHandler(Event currentEvent)
        {
            // Match name and path
            Console.WriteLine("In move handler");
            foreach (KeyValuePair<String, File> file in fileList)
            {
                Console.WriteLine("Trying to match {0} against {1}", file.Value.Path, currentEvent.File.Path);
                if (file.Key.Equals(currentEvent.File.Name) && file.Value.Path.Equals(currentEvent.File.Path))
                {
                    file.Value.Path = currentEvent.File.NewPath;
                    CopyFile(currentEvent.File);
                    sendPutRequest(file.Value);
                }
            }
        }

        private void ModifyHandler(Event currentEvent)
        {
            SendModifyRequest(currentEvent.File);
        }

        private void OpenHandler(Event currentEvent)
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

            // Send request to server
            SendOpenRequest(currentEvent.File);
        }

        private void DeleteHandler(Event currentEvent)
        {
            //if (IsTracked(currentEvent.File.Name))
            //{
                // delete from transient folder
                DeleteFile(currentEvent.File);

                // delete from the server
                sendDeleteRequest(currentEvent.File);

                // remove from internal data structure
                //fileList.Remove(currentEvent.File.Name);
            //}
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

            if (!SystemFile.Exists(destination) || SystemFile.GetLastWriteTime(destination) <= SystemFile.GetLastWriteTime(source) && Utilities.ExtensionIsSupported(source))
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

        private byte[] PostDataToServer(NameValueCollection data, String action)
        {
            byte[] response;
            using (WebClient webClient = new WebClient())
            {
                String url = String.Concat(Settings.apiUrl, action);
                Console.WriteLine("Posting to URL {0} ", url);
                response = webClient.UploadValues(url, data);
            }
            return response;
        }

        private void SendModifyRequest(File file)
        {
            NameValueCollection data = new NameValueCollection()
            {
                {"fileHash", "1"},
                {"fileName", file.Name},
                {"filePath", file.Path},
                {"fileLastModified", file.LastModified.ToShortTimeString()}
            };
            byte[] response = PostDataToServer(data, "modify/");
            if (response == null)
                Console.WriteLine("An Error occured while posting data to server");
            else
                Console.WriteLine("Successfully created file {0} on server", file.Name);
        }

        private void SendOpenRequest(File file)
        {
            NameValueCollection data = new NameValueCollection()
            {
                {"fileHash", "1"},
                {"fileName", file.Name},
                {"filePath", file.Path},
                {"EventName", "open"},
                {"LastAccessTime", DateTime.Now.ToString()}
            };
            byte[] response = PostDataToServer(data, "open/");
            if (response == null)
                Console.WriteLine("An Error occured while posting Open Event to server");
            else
                Console.WriteLine("Successfully sent open event on {0} to server", file.Name);
        }

        private void sendDeleteRequest(File file)
        {
            NameValueCollection data = new NameValueCollection()
            {
                {"fileHash", "1"},
                {"fileName", file.Name},
                {"filePath", file.Path},
                {"EventName", "delete"}
            };
            byte[] response = PostDataToServer(data, "delete/");
            if (response == null)
                Console.WriteLine("An Error occured while deleting data from the server");
            else
                Console.WriteLine("Successfully sent delete request for file {0} at location {1}", file.Name, file.Path);
        }

        private void sendPutRequest(File file)
        {
        }

        public Boolean IsTracked(String fileName)
        {
            return fileList.ContainsKey(fileName);
        }
    }
}
