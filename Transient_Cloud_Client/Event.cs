using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Transient_Cloud_Client
{
    class Event
    {
        public Event(String fileName, String filePath, Utilities.EVENT_ACTIONS action)
        {
            File file = new File(fileName, filePath);
            this.File = file;
            this.Action = action;
        }

        // Special constructor for move events
        public Event(String fileName, String oldPath, String newPath, Utilities.EVENT_ACTIONS action)
        {
            File file = new File(fileName, oldPath, newPath);
            this.File = file;
            this.Action = action;
        }

        private File file;
        public File File
        {
            get
            {
                return file;
            }
            set
            {
                file = value;
            }
        }

        private Utilities.EVENT_ACTIONS action;
        public Utilities.EVENT_ACTIONS Action
        {
            get
            {
                return action;
            }
            set
            {
                action = value;
            }
        }
    }
}
