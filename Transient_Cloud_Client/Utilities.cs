using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

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
    }
}
