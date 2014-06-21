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
            FileName = fileName;
            FilePath = filePath;
            Action = action;
        }
        private String filePath;
        private String fileName;
        public String FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
            }
        }
        
        public String FilePath
        {
            get
            {
                return filePath;
            }
            set
            {
                filePath = value;
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
