using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E3.ReactorManager.ReportsManager.Model.Data
{

    public delegate void ShowReportPreviewEventHandler(string reportFilePath);
    public delegate void ClearReportPreviewEventHandler();
    public delegate void ReportGenerationInProgressEventHandler();

    /// <summary>
    /// Additional Data to be printed on the Report
    /// </summary>
    public class LabelValuePair
    {
        public LabelValuePair(string label, string value)
        {
            Label = label; Value = value;
        }

        public string Label { get; set; }

        public string Value { get; set; }
    }
}
