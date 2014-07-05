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
    class FileSystemUtilities
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
                if (path.StartsWith(directory) && !path.StartsWith(Settings.transientCloudDirectoryPath))
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

        public static bool isTemporaryFile(String name)
        {
            return (name.StartsWith("$") || name.StartsWith("~") || (name[0] > '0' && !name.Contains(".")));
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

        public static void PutFileInTransientFolder(File file)
        {
            String sharedPath = CreateDirectories(file.Path);
            CopyFile(file, sharedPath);
        }

        public static String CreateDirectories(String path)
        {
            int startIndex = 0;
            for (int i = 0; i < path.Length; i++)
                for (int j = 0; j < Settings.directoriesToWatch.Length; j++)
                    if (path.Substring(0, i).Equals(Settings.directoriesToWatch[j]))
                    {
                        startIndex = i;
                        break;
                    }
            int lastIndex = path.LastIndexOf(@"\");
            String transientPath = String.Concat(Settings.transientCloudDirectoryPath, path.Substring(startIndex, lastIndex - startIndex + 1));
            Console.WriteLine(transientPath);
            Directory.CreateDirectory(transientPath);
            return transientPath;
        }

        public static void CopyFile(File file, String destination)
        {
            Console.WriteLine("Currently Processing: " + file.Name);
            destination = String.Concat(destination, file.Name);
            String source = file.Path;
            // Does destination file exist?
            // Don't copy if the last modified date of the file is the same as what is present in the directory

            if (!SystemFile.Exists(destination) || SystemFile.GetLastWriteTime(destination) <= SystemFile.GetLastWriteTime(source) && FileSystemUtilities.ExtensionIsSupported(source))
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

        public static void DeleteFile(String fullFilePath)
        {
            SystemFile.Delete(fullFilePath);
        }
        public static void MoveFile(String originalPath, String newPath)
        {
            SystemFile.Move(originalPath, newPath);
        }
        public static String GetTransientFolderPath(String path)
        {
            int startIndex = 0;
            for (int i = 0; i < path.Length; i++)
                for (int j = 0; j < Settings.directoriesToWatch.Length; j++)
                    if (path.Substring(0, i).Equals(Settings.directoriesToWatch[j]))
                        startIndex = i;
            String sharedPath = String.Concat(Settings.transientCloudDirectoryPath, extractRelativePathFromPath(path));
            return sharedPath;
        }

        public static int GenerateMD5Hash(File file)
        {
            using (var md5 = MD5.Create())
            {
                FileStream stream = new FileStream(file.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                Byte[] bytes = md5.ComputeHash(stream);
                return BitConverter.ToInt32(bytes, 0);
            }
        }
        public static String extractRelativePathFromPath(String path)
        {
            foreach (String directoryToWatch in Settings.directoriesToWatch)
                if (path.Contains(directoryToWatch))
                    return (path.Replace(directoryToWatch, ""));
            return path;
        }
        public static String ExtractNameFromPath(String path)
        {
            return path.Substring(path.LastIndexOf(@"\") + 1);
        }
    }
}
