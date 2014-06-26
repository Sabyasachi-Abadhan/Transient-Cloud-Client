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
            sendPutRequest(currentEvent.File);
        }

        private void MoveHandler(Event currentEvent)
        {
            // Match name and path
            Console.WriteLine("In move handler");
            FileSystemUtilities.CopyFile(currentEvent.File);
            sendPutRequest(currentEvent.File);
        }

        private void ModifyHandler(Event currentEvent)
        {
            SendModifyRequest(currentEvent.File);
        }

        private void OpenHandler(Event currentEvent)
        {
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
                Console.WriteLine("An Error occured while putting data to the server");
            else
                Console.WriteLine("Successfully sent put request changing {0} {1}", file.Path, file.NewPath);
        }

    }
}
