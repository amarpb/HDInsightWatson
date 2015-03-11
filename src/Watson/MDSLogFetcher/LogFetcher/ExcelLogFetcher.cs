using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;


namespace ClusterLogAnalyzer
{
    public class ExcelLogFetcher : ILogFetcher
    {
        private string path;
        public ExcelLogFetcher(string filePath)
        {
            path = filePath;
        }

        public DataTable Fetch()
        {
            // return FetchUsingOledb();
            return FetchUsingExcelInterop();
        }
        public DataTable FetchUsingExcelInterop()
        {
            DataTable table = new DataTable();
            var app = new Excel.Application();
            var workBooks = app.Workbooks;
            var workbook = workBooks.Open(path);
            var workSheet = (Excel.Worksheet)workbook.ActiveSheet;

            // Get range of the worksheet
            Excel.Range rg = workSheet.UsedRange;

            object[,] data = rg.Value2;

            // Add headers
            for (int i = 1; i <= data.GetLength(1); i++)
            {
                table.Columns.Add((string)data[1, i]);
            }
            // Add rows

            for (int i = 2; i <= data.GetLength(0); i++)
            {
                DataRow row = table.NewRow();
                for (int j = 1; j <= data.GetLength(1); j++)
                {
                    object content = data[i,j] ?? "";
                    row[j - 1] = content.ToString().Trim();
                }
                table.Rows.Add(row);
            }

            Marshal.FinalReleaseComObject(rg);
            Marshal.FinalReleaseComObject(workSheet);
            workbook.Close();
            Marshal.FinalReleaseComObject(workbook);
            Marshal.FinalReleaseComObject(workBooks);
            app.Quit();
            Marshal.FinalReleaseComObject(app);
            return table;
        }

        public DataTable FetchUsingOledb()
        {
            string connStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source='" + path + "';Extended Properties='Excel 12.0;HDR=Yes';";
            OleDbConnection conn = new OleDbConnection(connStr);
            conn.Open();

            OleDbDataAdapter da = new OleDbDataAdapter("select * from [Sheet1$]", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);

            return dt;
        }
    }
}
