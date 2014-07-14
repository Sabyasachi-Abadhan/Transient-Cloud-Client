using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Transient_Cloud_Client
{
    /// <summary>
    /// On initialization of the app, it reads settings from the App.Config and puts them into this class.
    /// Fields of this app are then used throughout the application
    /// </summary>
    class Settings
    {
        public static String[] directoriesToWatch;
        public static String apiUrl;
        public static String[] defaultDirectoriesToWatch;
        public static String dropboxDirectory;
        public static String transientCloudDirectoryName;
        public static String transientCloudDirectoryPath;
    }
}
