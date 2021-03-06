﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Transient_Cloud_Client
{
    /// <summary>
    /// Class is a model of a system event such as move, rename etc.
    /// </summary>
    class Event
    {
        public Event(String fileName, String filePath, FileSystemUtilities.EVENT_ACTIONS action)
        {
            File file = new File(fileName, filePath);
            this.File = file;
            this.Action = action;
        }

        // Special constructor for move, rename events
        public Event(String fileName, String oldPath, String newPath, FileSystemUtilities.EVENT_ACTIONS action)
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

        private FileSystemUtilities.EVENT_ACTIONS action;
        public FileSystemUtilities.EVENT_ACTIONS Action
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
