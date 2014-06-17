using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Transient_Cloud_Client
{
    class DocumentMonitor
    {
        public static ArrayList GetOpenedWordFiles()
        {
            ArrayList documentList = new ArrayList();
            try
            {
                Microsoft.Office.Interop.Word.Application application = (Microsoft.Office.Interop.Word.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application");
                foreach (Microsoft.Office.Interop.Word.Document document in application.Documents)
                    documentList.Add(document.FullName);
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
                    workBookList.Add(workbook.FullName);
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
                    presentationList.Add(presentation.FullName);
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
            foreach(String directory in directoriesToWatch)
                sublist.AddRange(list.Cast<String>().Where(f => f.StartsWith(directory) == true).ToList());
            return sublist;
        }

        public static void monitor()
        {
            while (true)
            {
                ArrayList list;
                list = GetOpenedWordFiles();
                list.AddRange(GetOpenedExcelFiles());
                list.AddRange(GetOpenedPowerpointFiles());
                // Shared Queue Logic Will go here instead of the print
                Print(list);
            }
        }
    }
}