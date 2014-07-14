using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Transient_Cloud_Client
{
    /// <summary>
    /// Abstraction of files in the filesystem based on certain specific metadata such as name, path, last modified.
    /// </summary>
    class File
    {
        public File(String name, String path)
        {
            this.Name = name;
            this.Path = path;
            this.LastModified = System.IO.File.GetLastWriteTime(path);
        }

        // This constructor is only used by the move event
        public File(String name, String originalPath, String newPath)
        {
            this.Name = name;
            this.Path = originalPath;
            this.NewPath = newPath;
            this.LastModified = System.IO.File.GetLastWriteTime(path);
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

        private String newPath;
        public String NewPath
        {
            get
            {
                return newPath;
            }
            set
            {
                newPath = value;
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
            this.LastModified = System.IO.File.GetLastWriteTime(path);
        }
    }
}
