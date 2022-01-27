namespace E3.ReactorManager.ReportsManager.Model.Data
{
    public class ReportSection
    {
        public string Title { get; set; } = string.Empty;

        public object Data { get; set; }

        public SectionalDataType DataType { get; set; }

        public bool EndPageHere { get; set; }
    }

    public enum SectionalDataType
    {
        LabelValuePairs,
        Tablular,
        Image
    }
}
