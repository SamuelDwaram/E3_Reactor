using E3.ReactorManager.ReportsManager.Model.Data;
using System.Collections.Generic;

namespace E3.ReactorManager.ReportsManager.Model.Interfaces
{
    public interface IReportPrinter
    {
        event ShowReportPreviewEventHandler ShowReportPreviewEvent;
        event ClearReportPreviewEventHandler ClearReportPreviewEvent;
        event ReportGenerationInProgressEventHandler ReportGenerationInProgressEvent;

        void ClearReportPreview();

        void PrintReportSections(string reportHeader, IList<ReportSection> sections, string reportLogoPath = null);
    }
}
