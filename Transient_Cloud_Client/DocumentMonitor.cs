using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace Transient_Cloud_Client
{
    class DocumentMonitor
    {
        private ConcurrentQueue<Event> events;
        public DocumentMonitor(ConcurrentQueue<Event> events)
        {
            this.events = events;
        }
        public static ArrayList GetOpenedWordFiles()
        {
            ArrayList documentList = new ArrayList();
            try
            {
                Microsoft.Office.Interop.Word.Application application = (Microsoft.Office.Interop.Word.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application");
                foreach (Microsoft.Office.Interop.Word.Document document in application.Documents)
                    documentList.Add(new Event(document.FullName, document.Path, "open"));
            }
            catch { }
            return documentList;
        }

        public static ArrayList GetOpenedExcelFiles()
        {
            ArrayList workBookList = new ArrayList();
            try
            {
                Microsoft.Office.Interop.Excel.Application application = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
                foreach (Microsoft.Office.Interop.Excel.Workbook workbook in application.Workbooks)
                    workBookList.Add(new Event(workbook.FullName, workbook.Path, "open"));
            }
            catch { }
            return workBookList;
        }

        public static ArrayList GetOpenedPowerpointFiles()
        {
            ArrayList presentationList = new ArrayList();
            try
            {
                Microsoft.Office.Interop.PowerPoint.Application application = (Microsoft.Office.Interop.PowerPoint.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("PowerPoint.Application");
                foreach (Microsoft.Office.Interop.PowerPoint.Presentation presentation in application.Presentations)
                    presentationList.Add(new Event(presentation.FullName, presentation.Path, "open"));
            }
            catch { }
            return presentationList;
        }

        public static void Print(ArrayList list)
        {
            foreach (String item in list)
                Console.WriteLine(item);
        }

        // Need to fix this method and write some tests for him
        public static ArrayList FilterList(ArrayList list)
        {
            String[] directoriesToWatch = System.Configuration.ConfigurationManager.AppSettings["DirectoriesToWatch"].Split(',');
            ArrayList sublist = new ArrayList();
            foreach (String directory in directoriesToWatch)
                sublist.AddRange(list.Cast<String>().Where(f => f.StartsWith(directory) == true).ToList());
            return sublist;
        }

        public void monitor()
        {
            while (true)
            {
                ArrayList list;
                list = GetOpenedWordFiles();
                list.AddRange(GetOpenedExcelFiles());
                list.AddRange(GetOpenedPowerpointFiles());
                //Print(list);
                foreach (Event currentEvent in list)
                    events.Enqueue(currentEvent);
                list.Clear();
            }
        }
    }
}