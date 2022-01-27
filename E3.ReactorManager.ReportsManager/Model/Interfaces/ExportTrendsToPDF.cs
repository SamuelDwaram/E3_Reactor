using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using LiveCharts.Wpf;

namespace E3.ReactorManager.Interfaces.ReportsManager
{
    public class ExportTrendsToPDF
    {
        public void ExportTrends(CartesianChart chartVisual, string strHeader)
        {
            //open a dialog for letting user choose where
            //to save the file
            FileStream fileStream;
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Save Live Trends",
                Filter = "PDF files (*.pdf)|*.pdf",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if ((fileStream = (System.IO.FileStream)saveFileDialog.OpenFile()) != null)
                {
                    Document document = new Document();
                    document.SetPageSize(PageSize.A4);
                    PdfWriter writer = PdfWriter.GetInstance(document, fileStream);
                    document.Open();

                    //Report Header
                    BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    Font fntHead = new Font(bfntHead, 16, 1, iTextSharp.text.BaseColor.GRAY);
                    Paragraph prgHeading = new Paragraph
                    {
                        Alignment = Element.ALIGN_CENTER
                    };
                    prgHeading.Add(new Chunk(strHeader.ToUpper(), fntHead));
                    document.Add(prgHeading);

                    //Add a line seperation
                    Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.BaseColor.GRAY, Element.ALIGN_LEFT, 1)));
                    document.Add(p);

                    //Add line break
                    document.Add(new Chunk("\n", fntHead));

                    SaveToPng(chartVisual, "C:/ReactorTrends.png");

                    iTextSharp.text.Image pngImage = iTextSharp.text.Image.GetInstance("C:/ReactorTrends.png");

                    //resize image upon your need
                    pngImage.ScaleToFit(500f, 500f);

                    //align the image
                    pngImage.Alignment = Element.ALIGN_CENTER;

                    document.Add(pngImage);

                    document.Close();
                    writer.Close();

                    fileStream.Close();
                }
            }
        }

        public void SaveToPng(FrameworkElement visual, string fileName)
        {
            var encoder = new PngBitmapEncoder();
            EncodeVisual(visual, fileName, encoder);
        }

        private static void EncodeVisual(FrameworkElement visual, string fileName, BitmapEncoder encoder)
        {
            var bitmap = new RenderTargetBitmap((int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            var frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);
            using (var stream = File.Create(fileName)) encoder.Save(stream);
        }
    }
}
