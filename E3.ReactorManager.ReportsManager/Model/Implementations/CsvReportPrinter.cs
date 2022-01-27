using E3.ReactorManager.ReportsManager.Model.Data;
using E3.ReactorManager.ReportsManager.Model.Interfaces;
using E3Tech.IO.FileAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace E3.ReactorManager.ReportsManager.Model.Implementations
{
    public class CsvReportPrinter : ICsvReportPrinter
    {
        private readonly IFileBrowser fileBrowser = new DefaultFileBrowser();
        public event ShowReportPreviewEventHandler ShowReportPreviewEvent;
        public event ClearReportPreviewEventHandler ClearReportPreviewEvent;
        public event ReportGenerationInProgressEventHandler ReportGenerationInProgressEvent;

        public void ClearReportPreview()
        {
            ClearReportPreviewEvent?.BeginInvoke(null, null);
        }

        public void PrintReportSections(string reportHeader, IList<ReportSection> sections)
        {
            string fileName = fileBrowser.SaveFile("Report1", ".csv");
            while (true)
            {
                if (IsFileLocked(fileName))
                {
                    MessageBox.Show("File is being already used. Please select another file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    fileName = fileBrowser.SaveFile("Report1", ".csv");
                }
                else
                {
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                //Skip.
            }
            else
            {
                //Invoke report generation in progress 
                ReportGenerationInProgressEvent?.Invoke();

                CreateCsv(fileName, reportHeader);

                foreach (ReportSection reportSection in sections)
                {
                    switch (reportSection.DataType)
                    {
                        case SectionalDataType.LabelValuePairs:
                            AddLabelValuePairs(reportSection, fileName);
                            break;
                        case SectionalDataType.Tablular:
                            AddTable(reportSection, fileName);
                            break;
                        default:
                            break;
                    }
                }

                ShowReportPreviewEvent?.BeginInvoke(fileName, null, null);
            }
        }

        private void AddTable(ReportSection reportSection, string fileName)
        {
            DataTable dataTable = (DataTable)reportSection.Data;
            IList<string[]> data = dataTable.AsEnumerable().Select(dataRow => dataRow.ItemArray.Select(item => Convert.ToString(item)).ToArray()).ToList();
            IList<string> columnNames = new List<string>();
            foreach (DataColumn column in dataTable.Columns)
            {
                columnNames.Add(column.ColumnName);
            }
            data.Insert(0, columnNames.ToArray());
            WriteToCsv(data.ToArray(), fileName);
        }

        private void AddLabelValuePairs(ReportSection reportSection, string fileName)
        {
            WriteToCsv(((IList<LabelValuePair>)reportSection.Data).Select(labelValuePair => new string[] { 
                labelValuePair.Label, labelValuePair.Value
            }).ToArray(), fileName);
        }

        private void CreateCsv(string fileName, string reportHeader)
        {
            if (File.Exists(fileName))
            {
                File.Create(fileName);
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(reportHeader);
            File.WriteAllText(fileName, stringBuilder.ToString());
        }

        private void WriteToCsv(IEnumerable<IEnumerable<string>> data, string fileName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (IEnumerable<string> dataRow in data)
            {
                stringBuilder.AppendLine(string.Join(",", dataRow));
            }
            File.WriteAllText(fileName, stringBuilder.ToString());
        }

        protected virtual bool IsFileLocked(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        stream.Close();
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }
    }
}
