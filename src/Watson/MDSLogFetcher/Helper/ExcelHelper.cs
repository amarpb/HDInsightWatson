using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Office.Interop.Excel;
using DataTable = System.Data.DataTable;
using System.Data.Services.Client;
using System.Data.Services.Common;

namespace ClusterLogAnalyzer
{
    public class ExcelHelper
    {

        public static bool ExportToExcel(System.Data.DataTable tbl, string excelFilePath = null)
        {
            try
            {
                if (tbl == null || tbl.Columns.Count == 0)
                {
                    return false;
                }


                // load excel, and create a new workbook
                var excelApp = new Application();
                var excelWorkbooks = excelApp.Workbooks;
                excelWorkbooks.Add();

                // single worksheet
                _Worksheet workSheet = (_Worksheet)excelApp.ActiveSheet;
                int fieldCount = tbl.Columns.Count;

                // column headings
                for (int i = 0; i < fieldCount; i++)
                {
                    workSheet.Cells[1, (i + 1)] = tbl.Columns[i].ColumnName;
                }

                // Max number of rows supported by excel 2013 are 1,048,576
                int tableRowCount = Math.Min(148575, tbl.Rows.Count);
                object[,] tableValues = new object[tableRowCount, tbl.Columns.Count];
                // rows
                for (int i = 0; i < tableRowCount; i++)
                {
                    for (int j = 0; j < tbl.Columns.Count; j++)
                    {
                        tableValues[i, j] = " " + tbl.Rows[i][j].ToString();
                    }
                }

                // Get the first and last cells of the current row.
                var firstColumnCell = workSheet.Cells[2, 1];
                var lastColumnCell = workSheet.Cells[tableRowCount + 1, tbl.Columns.Count];
                // Set the row
                var range = workSheet.Range[firstColumnCell, lastColumnCell];
                range.Value2 = tableValues;

                Marshal.FinalReleaseComObject(firstColumnCell);
                Marshal.FinalReleaseComObject(lastColumnCell);
                Marshal.FinalReleaseComObject(range);

                Range cells = workSheet.Cells;
                Range last = cells.SpecialCells(XlCellType.xlCellTypeLastCell, Type.Missing);

                //Add some formatting
                Range tableRange = workSheet.get_Range("A1", last);

                var objects = workSheet.ListObjects;
                objects.Add(XlListObjectSourceType.xlSrcRange, tableRange, Type.Missing, XlYesNoGuess.xlYes, Type.Missing).Name = "TestTable";

                var listObjects = workSheet.ListObjects["TestTable"];
                listObjects.TableStyle = "TableStyleMedium2";

                const double maxSize = 80.43;

                for (int i = 0; i < fieldCount; i++)
                {
                    var column = ((Range)workSheet.Cells[1, i + 1]);
                    var entireColumn = column.EntireColumn;
                    entireColumn.AutoFit();

                    if ((double)column.ColumnWidth > maxSize)
                    {
                        column.ColumnWidth = maxSize;
                    }
                    Marshal.FinalReleaseComObject(entireColumn);
                    Marshal.FinalReleaseComObject(column);
                }

                Marshal.FinalReleaseComObject(listObjects);
                Marshal.FinalReleaseComObject(objects);
                Marshal.FinalReleaseComObject(tableRange);
                Marshal.FinalReleaseComObject(last);
                Marshal.FinalReleaseComObject(cells);


                // check fielpath
                if (!string.IsNullOrEmpty(excelFilePath))
                {
                    try
                    {
                        string dirName = Path.GetDirectoryName(excelFilePath);
                        if(false == Directory.Exists(dirName))
                        {
                            Directory.CreateDirectory(dirName);
                        }
                        // Check if file already exist. If so, increment the id
                        if (File.Exists(excelFilePath))
                        {
                            int dotIndex = excelFilePath.LastIndexOf('.');
                            excelFilePath = excelFilePath.Substring(0, dotIndex) + DateTime.Now.Ticks.ToString() + excelFilePath.Substring(dotIndex);
                        }
                        workSheet.SaveAs(excelFilePath);

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        excelWorkbooks.Close();

                        Marshal.FinalReleaseComObject(excelWorkbooks);
                        Marshal.FinalReleaseComObject(workSheet);
                        excelApp.Quit();
                        Marshal.FinalReleaseComObject(excelApp);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("ExportToExcel: Excel file could not be saved! Check filepath.\n"
                            + ex.Message);
                    }
                }
                else    // no filepath is given
                {
                    excelApp.Visible = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ExportToExcel: \n" + ex.Message);
            }

            return true;
        }
    }
}
