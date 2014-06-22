using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Transient_Cloud_Client
{
    class File
    {
        public File(String name, String path)
        {
            this.Name = name;
            this.Path = path;
            this.LastModified = System.IO.File.GetLastAccessTime(path);
        }
        private String name;
        public String Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        private String path;
        public String Path
        {
            get
            {
                return path;
            }
            set
            {
                path = value;
            }
        }
        private DateTime lastModified;
        public DateTime LastModified
        {
            get
            {
                return lastModified;
            }
            set
            {
                lastModified = value;
            }
        }
        public void UpdateLastModified()
        {
            LastModified = DateTime.Now;
        }
    }
}
