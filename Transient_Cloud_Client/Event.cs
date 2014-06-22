using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Transient_Cloud_Client
{
    class Event
    {
        public Event(String fileName, String filePath, String action)
        {
            File file = new File(fileName, filePath);
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

        private String action;
        public String Action
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
