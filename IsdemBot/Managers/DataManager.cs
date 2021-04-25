using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IsdemBot.Managers
{
    public static class DataManager
    {
        private static string _lastIndexTxtPath = "lastIndex.txt";

        public static IList<string> GetExcelList(string filePath, Models.ExcelColumn column)
        {
            List<string> tcNoList = new List<string>();

            using var package = new ExcelPackage(new FileInfo(filePath));

            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows;
            var columnCount = worksheet.Dimension.Columns;

            for (int row = 1; row <= rowCount; row++)
            {
                tcNoList.Add(worksheet.Cells[row, (int)column].Value.ToString());
                //for (int col = 1; col <= columnCount; col++)
                //{
                //}
            }

            return tcNoList;
        }

        public static void SetLastIndex(int index)
        {
            if (File.Exists(_lastIndexTxtPath))
            {
                using StreamWriter fs = new(_lastIndexTxtPath);
                fs.WriteLine(index);
            }
            else
            {
                using FileStream fs = File.Create(_lastIndexTxtPath);
                byte[] info = new UTF8Encoding(true).GetBytes(index.ToString());
                fs.Write(info, 0, info.Length);
            }
        }

        public static int ReadLastIndex()
        {
            try
            {
                using StreamReader sr2 = new StreamReader(_lastIndexTxtPath);
                string fileText = sr2.ReadToEnd();

                if (string.IsNullOrWhiteSpace(fileText))
                    fileText = "0";

                return int.Parse(fileText);
            }
            catch
            {
                return 0;
            }
        }
    }
}