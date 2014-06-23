using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace Transient_Cloud_Client
{
    class Utilities
    {
        public enum EVENT_ACTIONS
        {
            open,
            delete,
            rename,
            move
        }

        public static String[] supportedExtensions = { ".docx", ".doc", ".rtf", ".odt", ".dotm", ".xls", ".xlsx", ".ppt", ".pptx" };
        public static ArrayList FilterList(ArrayList list)
        {
            ArrayList sublist = new ArrayList();
            foreach (Event currentEvent in list)
                if (fileIsImportant(currentEvent.File.Path))
                    sublist.Add(currentEvent);
            return sublist;
        }

        public static bool fileIsImportant(String path)
        {
            foreach (String directory in Settings.directoriesToWatch)
                if (path.StartsWith(directory))
                    return true;
            return false;
        }

        public static bool ExtensionIsSupported(String path)
        {
            foreach (String extension in supportedExtensions)
                if (Path.GetExtension(path).Equals(extension))
                    return true;
            return false;
        }

        public static bool IsServerOnline()
        {
            String responseText;
            String address = String.Concat(Settings.apiUrl, "status");
            using (WebClient client = new WebClient())
            {
                responseText = client.DownloadString(address);
            }
            if (responseText.Equals("Server is running"))
                return true;
            return false;
        }
    }
}
