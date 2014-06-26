using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using SystemFile = System.IO.File;
using System.Net;
using System.Security.Cryptography;

namespace Transient_Cloud_Client
{
    class Utilities
    {
        public enum EVENT_ACTIONS
        {
            open,
            close,
            delete,
            modify,
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

        public static int GenerateMD5Hash(File file)
        {
            using (var md5 = MD5.Create())
            {
                FileStream stream = new FileStream(file.Path, FileMode.Open, FileAccess.Read);
                Byte[] bytes = md5.ComputeHash(stream);
                return BitConverter.ToInt32(bytes, 0);
            }
        }

    }
}
