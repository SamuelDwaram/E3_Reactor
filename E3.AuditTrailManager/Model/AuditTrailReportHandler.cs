using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.ReportsManager.Model.Data;
using E3.ReactorManager.ReportsManager.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace E3.AuditTrailManager.Model
{
    public class AuditTrailReportHandler
    {
        private readonly IReportPrinter reportPrinter;
        private readonly IDatabaseReader databaseReader;

        public AuditTrailReportHandler(IReportPrinter reportPrinter, IDatabaseReader databaseReader)
        {
            this.reportPrinter = reportPrinter;
            this.databaseReader = databaseReader;
        }

        public void PrintAuditTrailReport(DateTime selectedStartTime, DateTime selectedEndTime)
        {
            Task.Factory.StartNew(new Func<IList<ReportSection>>(() => GetAuditTrailBetweenTimeStamps(selectedStartTime, selectedEndTime)))
                .ContinueWith(new Action<Task<IList<ReportSection>>>(t => reportPrinter.PrintReportSections("REPORT", t.Result, Path.Combine(AppDomain.CurrentDomain.BaseDirectory + @"Images\report_logo.png"))));
        }

        private IList<ReportSection> GetAuditTrailBetweenTimeStamps(DateTime startTime, DateTime endTime)
        {
            DataTable auditRecords = new DataTable();
            auditRecords.Columns.Add("TIME STAMP", typeof(string));
            auditRecords.Columns.Add("ACTION TYPE", typeof(string));
            auditRecords.Columns.Add("MESSAGE", typeof(string));
            auditRecords.Columns.Add("USER", typeof(string));
            databaseReader.ExecuteReadCommand($"select * from dbo.AuditTrail where TimeStamp between '{startTime:yyyy-MM-dd HH:mm:ss}' and '{endTime:yyyy-MM-dd HH:mm:ss}' order by TimeStamp", CommandType.Text).AsEnumerable().ToList()
                .ForEach(dataRecord => {
                    auditRecords.Rows.Add(new List<object> {
                        dataRecord.Field<DateTime>("TimeStamp").ToString("HH:mm:ss dd-MM-yyyy"),
                        dataRecord.Field<string>("Category"),
                        dataRecord.Field<string>("AuditMessage"),
                        dataRecord.Field<string>("ScientistName"),
                    }.ToArray());
                });
            return new List<ReportSection>
            {
                new ReportSection
                {
                    Title = "AUDIT TRAIL",
                    Data = auditRecords,
                    DataType = SectionalDataType.Tablular,
                }
            };
        }
    }
}
