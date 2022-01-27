using System;

namespace E3.ReactorManager.ReportsManager.Model.Data
{
    public class PdfDetails
    {
        public string Identifier { get; set; }

        public string Name { get; set; }

        public byte[] Content { get; set; }

        public DateTime ReviewDate { get; set; }
    }
}
