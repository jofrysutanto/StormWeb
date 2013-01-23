using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OfficeOpenXml;

namespace StormWeb.Models.Report
{
    public class ReportModel
    {
        List<string> header = new List<string>();

        List<string[]> data = new List<string[]>();

        public ReportModel(List<string> header)
        {
            this.header = header;
        }

        public ReportModel(string[] header)
        {
            foreach (string s in header)
            {
                this.header.Add(s);
            }
        }

        public void addData(string[] record)
        {
            if (record.Length != header.Count)
                throw new Exception("Header and row count are different");

            data.Add(record);
        }

        public byte[] makeReport()
        {
            if (header.Count <= 0)
                throw new Exception("Incorrect report model format: No header defined");

            return new Report().makeReport(this);
        }

        private class Report
        {
            private ExcelPackage template = new ExcelPackage();
        
            public const int RESPONSE_BINARY = 1;

            public bool dateRestricted = false;
            private ExcelWorksheet _ws = null;

            public int INIT_ROW = 1;
            public int INIT_COL = 1;

            public DateTime startDate = DateTime.Now.AddYears(-1);
            public DateTime endDate = DateTime.Now;

            public byte [] makeReport(ReportModel rm)
            {
                this.addHeader(rm.header.ToArray());

                this.addDataList(rm.data);

                return template.GetAsByteArray();
            }

            private void addDataList(List<string[]> list)
            {
                ExcelWorksheet ws = this.getWorksheet();

                int row = INIT_ROW + 1; // Header is at row 1
                int col = INIT_COL;

                foreach (string[] sArr in list)
                {
                    foreach (string s in sArr)
                    {
                        ws.Cells[row, col].Value = s;
                        col++;   
                    }

                    col = INIT_COL;
                    row++;
                }
            }

            private ExcelWorksheet getWorksheet()
            {
                if (_ws == null)
                {
                    _ws = template.Workbook.Worksheets.Add("Worksheet 1");
                }

                return _ws;
            }

            public void addHeader(string [] headers, bool autoFilter = true)
            {
                ExcelWorksheet ws = getWorksheet();

                int row = INIT_ROW;
                int col = INIT_COL;

                foreach (string s in headers)
                {
                    ws.Cells[row, col].Value = s;
                    col++;
                }

                ws.Cells[1, 1, row, col-1].AutoFilter = true;
            }
        }
    }
}