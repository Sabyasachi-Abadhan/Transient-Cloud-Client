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
                case FileSystemUtilities.EVENT_ACTIONS.open:
                    OpenHandler(currentEvent);
                    break;
                case FileSystemUtilities.EVENT_ACTIONS.delete:
                    DeleteHandler(currentEvent);
                    break;
                case FileSystemUtilities.EVENT_ACTIONS.rename:
                    RenameHandler(currentEvent);
                    break;
                case FileSystemUtilities.EVENT_ACTIONS.move:
                    MoveHandler(currentEvent);
                    break;
                case FileSystemUtilities.EVENT_ACTIONS.modify:
                    ModifyHandler(currentEvent);
                    break;
            }
        }

        private void RenameHandler(Event currentEvent)
        {
            // Have to stop and consider what if the file is not in the database?
            FileSystemUtilities.PutFileInTransientFolder(currentEvent.File);
            sendPutRequest(currentEvent.File);
        }

        private void MoveHandler(Event currentEvent)
        {
            // Have to stop and consider what if the file is not in the database? Should first send the request. 
            // If the server says it's unimportant we can skip it
            
            sendPutRequest(currentEvent.File);
            String originalTransientFolderPath = FileSystemUtilities.GetTransientFolderPath(currentEvent.File.Path);
            Console.WriteLine("Original Path: " + originalTransientFolderPath);
            String newTransientFolderPath = FileSystemUtilities.GetTransientFolderPath(currentEvent.File.NewPath);
            Console.WriteLine("New Path: " + newTransientFolderPath);
            // If the original file existed then move it and send a put request otherwise do nothing
            if (!SystemFile.Exists(originalTransientFolderPath))
                return;
            // Create the directories of destination if doesn't exist
            String directories = newTransientFolderPath.Substring(0, newTransientFolderPath.LastIndexOf(@"\"));
            System.IO.Directory.CreateDirectory(directories);
            FileSystemUtilities.MoveFile(originalTransientFolderPath, newTransientFolderPath);
        }

        private void ModifyHandler(Event currentEvent)
        {
            FileSystemUtilities.PutFileInTransientFolder(currentEvent.File);
            SendModifyRequest(currentEvent.File);
        }

        private void OpenHandler(Event currentEvent)
        {
            SendOpenRequest(currentEvent.File);
        }

        private void DeleteHandler(Event currentEvent)
        {
            // No delete events are forwarded to the server because server manages deletions based on expiration date
        }

        private byte[] PostDataToServer(NameValueCollection data, String action)
        {
            byte[] response;
            using (WebClient webClient = new WebClient())
            {
                String url = String.Concat(Settings.apiUrl, action);
                //Console.WriteLine("Posting to URL {0} ", url);
                response = webClient.UploadValues(url, data);
            }
            return response;
        }

        private void SendModifyRequest(File file)
        {
            Console.WriteLine(FileSystemUtilities.GetTransientFolderPath(file.Path));
            NameValueCollection data = new NameValueCollection()
            {
                {"hash", FileSystemUtilities.GenerateMD5Hash(file).ToString()},
                {"name", file.Name},
                {"path", FileSystemUtilities.GetTransientFolderPath(file.Path)},
                {"expiration_date", file.LastModified.ToShortTimeString()}
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
                {"file_hash", "1"},
                {"file_name", file.Name},
                {"file_path", file.Path},
                {"date", DateTime.Now.ToString()}
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

        private bool sendPutRequest(File file)
        {
            // Send the old path and the new path, that's it ;)
            NameValueCollection data = new NameValueCollection()
            {
                {"fileHash", "1"},
                {"fileOldPath", file.Path},
                {"fileNewPath", file.NewPath},
                {"EventName", "put"}
            };
            byte[] response = PostDataToServer(data, "put/");
            if (response == null)
                return false;
            else
            {
                String responseString = System.Text.Encoding.Default.GetString(response);
                return responseString.Equals("Exists") ? true : false;
            }
        }
    }
}