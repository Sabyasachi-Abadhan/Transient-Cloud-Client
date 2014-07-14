using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace Transient_Cloud_Client
{
    /// <summary>
    /// Class uses Microsoft PIA to establish open documents/sheets/slide shows. It then creates open events for them and puts them onto the shared concurrent queue
    /// </summary>
    class DocumentMonitor
    {
        private ConcurrentQueue<Event> events;
        private ArrayList openedFiles = new ArrayList();
        public DocumentMonitor(ref ConcurrentQueue<Event> events)
        {
            this.events = events;
        }

        /// <summary>
        /// Returns a list of files open in Microsoft Word
        /// </summary>
        /// <returns>ArrayList of open word documents</returns>
        public static ArrayList GetOpenedWordFiles()
        {
            ArrayList documentList = new ArrayList();
            try
            {
                Microsoft.Office.Interop.Word.Application application = (Microsoft.Office.Interop.Word.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application");
                foreach (Microsoft.Office.Interop.Word.Document document in application.Documents)
                    documentList.Add(new Event(document.Name, document.FullName, FileSystemUtilities.EVENT_ACTIONS.open));
            }
            catch { }
            return documentList;
        }

        /// <summary>
        /// Returns a list of files open in Microsoft Excel
        /// </summary>
        /// <returns>ArrayList of open Excel sheets</returns>
        public static ArrayList GetOpenedExcelFiles()
        {
            ArrayList workBookList = new ArrayList();
            try
            {
                Microsoft.Office.Interop.Excel.Application application = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
                foreach (Microsoft.Office.Interop.Excel.Workbook workbook in application.Workbooks)
                    workBookList.Add(new Event(workbook.Name, workbook.FullName, FileSystemUtilities.EVENT_ACTIONS.open));
            }
            catch { }
            return workBookList;
        }

        /// <summary>
        /// Returns a list of files open in Microsoft Powerpoint
        /// </summary>
        /// <returns>ArrayList of open Powerpoint presentations</returns>
        public static ArrayList GetOpenedPowerpointFiles()
        {
            ArrayList presentationList = new ArrayList();
            try
            {
                Microsoft.Office.Interop.PowerPoint.Application application = (Microsoft.Office.Interop.PowerPoint.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("PowerPoint.Application");
                foreach (Microsoft.Office.Interop.PowerPoint.Presentation presentation in application.Presentations)
                    presentationList.Add(new Event(presentation.Name, presentation.FullName, FileSystemUtilities.EVENT_ACTIONS.open));
            }
            catch { }
            return presentationList;
        }

        public static void Print(ArrayList list)
        {
            foreach (Event currentEvent in list)
                Console.WriteLine(currentEvent.File.Name);
        }

        /// <summary>
        /// This is the most important method that keeps polling for documents and adding them to the queue
        /// It reads only true open events by keeping track of file close events
        /// </summary>
        public void monitor()
        {
            while (true)
            {
                ArrayList list;
                bool isStillOpen = false;
                list = GetOpenedWordFiles();
                list.AddRange(GetOpenedExcelFiles());
                list.AddRange(GetOpenedPowerpointFiles());

                // remove multiple events for same open action and add newly opened files to openedFiles
                ArrayList listCopy = new ArrayList(list);
                foreach (Event currentEvent in listCopy)
                {
                    if (openedFiles.Contains(currentEvent.File.Path))
                        list.Remove(currentEvent);
                    else
                        openedFiles.Add(currentEvent.File.Path);
                }

                ArrayList openedFilesCopy = new ArrayList(openedFiles);

                // Close the files which are no longer open
                foreach (String filePath in openedFilesCopy)
                {
                    isStillOpen = false;
                    foreach (Event currentEvent in listCopy)
                        if (currentEvent.File.Path.Equals(filePath))
                            isStillOpen = true;
                    if (!isStillOpen)
                        openedFiles.Remove(filePath);
                }

                Console.WriteLine("Enqueueing: ");
                Print(FileSystemUtilities.FilterList(list));
                foreach (Event currentEvent in list)
                    events.Enqueue(currentEvent);
                list.Clear();
            }
        }
    }
}