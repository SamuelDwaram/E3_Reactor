using E3.ReactorManager.ReportsManager.Model.Interfaces;
using Prism.Mvvm;

namespace E3.ReactorManager.ReportsManager.ViewModels
{
    public class ReportPreviewViewModel : BindableBase
    {
        private readonly IReportPrinter reportPrinter;

        public ReportPreviewViewModel(IReportPrinter reportPrinter)
        {
            this.reportPrinter = reportPrinter;
            this.reportPrinter.ShowReportPreviewEvent += ReportPrinter_ShowReportPreview;
            this.reportPrinter.ClearReportPreviewEvent += ReportPrinter_ClearReportPreview;
            this.reportPrinter.ReportGenerationInProgressEvent += ReportPrinter_ReportGenerationInProgressEvent;
        }

        private void ReportPrinter_ReportGenerationInProgressEvent()
        {
            ReportGenerationInProgress = true;
        }

        private void ReportPrinter_ClearReportPreview()
        {
            ReportGenerationInProgress = false;
            ReportFilePath = string.Empty;
        }

        private void ReportPrinter_ShowReportPreview(string reportFilePath)
        {
            ReportGenerationInProgress = false;
            ReportFilePath = reportFilePath;
        }

        #region Properties
        private string _reportFilePath = string.Empty;
        public string ReportFilePath
        {
            get { return _reportFilePath; }
            set => SetProperty(ref _reportFilePath, value);
        }

        private bool _reportGenerationInProgress;
        public bool ReportGenerationInProgress
        {
            get { return _reportGenerationInProgress; }
            set { SetProperty(ref _reportGenerationInProgress, value); }
        }
        #endregion
    }
}
