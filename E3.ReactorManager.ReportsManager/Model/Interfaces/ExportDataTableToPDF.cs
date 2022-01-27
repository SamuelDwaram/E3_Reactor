using System.Data;
using System.Windows.Forms;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;
using E3.ReactorManager.Interfaces.DesignExperiment.Data;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System;
using System.Drawing.Imaging;

namespace E3.ReactorManager.Interfaces.ReportsManager
{
    public class ExportDataTableToPDF
    {
        public void ExportToPDF(DataTable batchDataTable,
                                DataTable actionCommentsTable,
                                DataTable reactorImagesTable,
                                Batch batchConfiguration)
        {
            //open a dialog for letting user choose where 
            //to save the file
            FileStream fileStream;
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Save Data Report",
                Filter = "PDF files (*.pdf)|*.pdf",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            try
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if ((fileStream = (System.IO.FileStream)saveFileDialog.OpenFile()) != null)
                    {
                        Document document = new Document(PageSize.A4, 50, 50, 80, 30);
                        //document.SetPageSize(PageSize.A4);
                        //document.SetMargins(40, 40, 40, 40);
                        PdfWriter writer = PdfWriter.GetInstance(document, fileStream);
                        BaseColor baseColor = new BaseColor(39, 43, 52);

                        //open document for writing
                        document.Open();
                        //using header class (for adding header in each page)
                        writer.PageEvent = new HeaderFooter();

                        //check whether batch configuration data is null or not
                        if (!string.IsNullOrEmpty(batchConfiguration.Name))
                        {
                            //BATCH DATA LEFT
                            Paragraph prgBatchDataLeft = new Paragraph();
                            BaseFont baseFontBatchData = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                            Font fontBatchData = new Font(baseFontBatchData, 13, 1, baseColor);

                            prgBatchDataLeft.Alignment = Element.ALIGN_LEFT;
                            prgBatchDataLeft.Add(new Chunk("\nPROJECT NAME : " + batchConfiguration.Name, fontBatchData));
                            prgBatchDataLeft.Add(new Chunk("\nBATCH NUMBER : " + batchConfiguration.Number, fontBatchData));
                            prgBatchDataLeft.Add(new Chunk("\nSCIENTIST NAME : " + batchConfiguration.ScientistName, fontBatchData));
                            prgBatchDataLeft.Add(new Chunk("\nREACTOR : " + batchConfiguration.FieldDeviceIdentifier, fontBatchData));
                            prgBatchDataLeft.Add(new Chunk("\nHEATING COOLING : " + batchConfiguration.HCIdentifier, fontBatchData));
                            prgBatchDataLeft.Add(new Chunk("\nSTIRRER : " + batchConfiguration.StirrerIdentifier, fontBatchData));
                            prgBatchDataLeft.Add(new Chunk("\nDOSING PUMP : " + batchConfiguration.DosingPumpUsage, fontBatchData));
                            prgBatchDataLeft.Add(new Chunk("\nCOMMENTS : " + batchConfiguration.Comments, fontBatchData));
                            prgBatchDataLeft.Add(new Chunk("\nTIME STARTED : " + batchConfiguration.TimeStarted, fontBatchData));
                            document.Add(prgBatchDataLeft);

                            document.Add(new Chunk("\n"));
                        }
                        
                        //batch Table header
                        BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        Font fntColumnHeader = new Font(btnColumnHeader, 10, 1, iTextSharp.text.BaseColor.WHITE);

                        if (batchDataTable.Rows.Count > 0)
                        {
                            //Write the table
                            PdfPTable batchData = new PdfPTable(batchDataTable.Columns.Count);
                            batchData.WidthPercentage = 100;
                            batchData.HeaderRows = 1;

                            //batch table header
                            for (int i = 0; i < batchDataTable.Columns.Count; i++)
                            {
                                PdfPCell columnHeaderCell = new PdfPCell
                                {
                                    BackgroundColor = baseColor
                                };
                                Paragraph columnHeaderContent
                                        = new Paragraph(new Chunk(batchDataTable.Columns[i].ColumnName.ToUpper(), fntColumnHeader))
                                        {
                                            Alignment = Element.ALIGN_CENTER
                                        };
                                columnHeaderCell.AddElement(columnHeaderContent);
                                columnHeaderCell.HorizontalAlignment = Element.ALIGN_CENTER;
                                columnHeaderCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                batchData.AddCell(columnHeaderCell);
                            }
                            //batch table Data
                            for (int i = 0; i < batchDataTable.Rows.Count; i++)
                            {
                                for (int j = 0; j < batchDataTable.Columns.Count; j++)
                                {
                                    BaseFont btnColumnValue = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                    Font fntColumnValue = new Font(btnColumnValue, 10, 1, baseColor);
                                    PdfPCell columnContentCell = new PdfPCell();
                                    Paragraph columnCellContent
                                        = new Paragraph(new Chunk(batchDataTable.Rows[i][j].ToString(), fntColumnValue))
                                        {
                                            Alignment = Element.ALIGN_CENTER
                                        };
                                    columnContentCell.AddElement(columnCellContent);
                                    columnContentCell.HorizontalAlignment = Element.ALIGN_CENTER;
                                    columnContentCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                    batchData.AddCell(columnContentCell);
                                }
                            }

                            document.Add(batchData);

                            document.NewPage(); //start ActionComments from New Page in the PDF
                        }
                        if (actionCommentsTable.Rows.Count > 0)
                        {

                            PdfPTable actionComments = new PdfPTable(actionCommentsTable.Columns.Count);
                            actionComments.WidthPercentage = 100;
                            actionComments.HeaderRows = 1;

                            //Action comments table header
                            for (int i = 0; i < actionCommentsTable.Columns.Count; i++)
                            {
                                PdfPCell columnHeaderCell = new PdfPCell
                                {
                                    BackgroundColor = baseColor
                                };
                                Paragraph columnHeaderContent
                                        = new Paragraph(new Chunk(actionCommentsTable.Columns[i].ColumnName.ToUpper(), fntColumnHeader))
                                        {
                                            Alignment = Element.ALIGN_CENTER
                                        };
                                columnHeaderCell.AddElement(columnHeaderContent);
                                columnHeaderCell.HorizontalAlignment = Element.ALIGN_CENTER;
                                columnHeaderCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                actionComments.AddCell(columnHeaderCell);
                            }
                            //Action comments table Data
                            for (int i = 0; i < actionCommentsTable.Rows.Count; i++)
                            {
                                for (int j = 0; j < actionCommentsTable.Columns.Count; j++)
                                {
                                    BaseFont btnColumnValue = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                    Font fntColumnValue = new Font(btnColumnValue, 10, 1, baseColor);
                                    PdfPCell columnContentCell = new PdfPCell();
                                    Paragraph columnCellContent
                                        = new Paragraph(new Chunk(actionCommentsTable.Rows[i][j].ToString(), fntColumnValue))
                                        {
                                            Alignment = Element.ALIGN_CENTER
                                        };
                                    columnContentCell.AddElement(columnCellContent);
                                    columnContentCell.HorizontalAlignment = Element.ALIGN_CENTER;
                                    columnContentCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                    actionComments.AddCell(columnContentCell);
                                }
                            }

                            document.Add(actionComments);

                            document.NewPage();//start Reactor Images from New Page in the PDF

                        }
                        if (reactorImagesTable.Rows.Count > 0)
                        {
                            BaseFont reactorImageBaseFontHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                            Font reactorImageFontHead = new Font(reactorImageBaseFontHead, 16, 1, iTextSharp.text.BaseColor.BLACK);
                            Paragraph reactorImagePrgHeading = new Paragraph
                            {
                                Alignment = Element.ALIGN_LEFT
                            };

                            reactorImagePrgHeading.Add(new Chunk(("REACTOR IMAGES"), reactorImageFontHead));

                            document.Add(reactorImagePrgHeading);

                            //start adding images from here
                            for (int dataRowCount = 0;
                                     dataRowCount < reactorImagesTable.Rows.Count;
                                     dataRowCount++)
                            {
                                //Reactor Image DATA LEFT
                                Paragraph reactorImageData = new Paragraph();
                                BaseFont baseFontReactorImageData = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                                Font fontReactorImageData = new Font(baseFontReactorImageData, 13, 1, baseColor);

                                reactorImageData.Alignment = Element.ALIGN_LEFT;
                                reactorImageData.Add(new Chunk("\nREACTOR NAME : " + reactorImagesTable.Rows[dataRowCount][0].ToString(), fontReactorImageData));
                                reactorImageData.Add(new Chunk("\nTIME CAPTURED : " + DateTime.Parse(reactorImagesTable.Rows[dataRowCount][2].ToString()), fontReactorImageData));
                                document.Add(reactorImageData);
                                //reactor image
                                System.Drawing.Image reactorImage
                                        = (System.Drawing.Bitmap)(new System.Drawing.ImageConverter()).ConvertFrom(reactorImagesTable.Rows[dataRowCount][1]);
                                iTextSharp.text.Image reportReactorImage
                                        = iTextSharp.text.Image.GetInstance(reactorImage, ImageFormat.Png);

                                //resize image upon your need
                                reportReactorImage.ScaleToFit(400f, 350f);

                                //align the image
                                reportReactorImage.Alignment = Element.ALIGN_LEFT;

                                document.Add(reportReactorImage);
                            }
                        }

                        //document.NewPage(); //start ReactorTrends Image from New Page in the PDF

                        //BaseFont reactorTrendsBaseFontHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        //Font reactorTrendsFontHead = new Font(reactorTrendsBaseFontHead, 16, 1, iTextSharp.text.BaseColor.GRAY);
                        //Paragraph prgHeading = new Paragraph
                        //{
                        //    Alignment = Element.ALIGN_LEFT
                        //};
                        //prgHeading.Add(new Chunk(("Reactor Trends"), reactorTrendsFontHead));

                        //document.Add(prgHeading);

                        //iTextSharp.text.Image trendsImage = iTextSharp.text.Image.GetInstance("C:/ReactorTrends.png");

                        ////resize image upon your need
                        //trendsImage.ScaleToFit(500f, 500f);

                        ////align the image
                        //trendsImage.Alignment = Element.ALIGN_CENTER;

                        //document.Add(trendsImage);

                        document.Close();
                        writer.Close();
                        fileStream.Close();
                    }
                }

            }
            catch (Exception ex)
            {
                //create error log file
                //Logger.LoggerInstance.LogFileWrite(Logger.LoggerInstance.CreateErrorMessage(ex));
                Console.WriteLine("Error in printing PDF");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void SaveToPng(FrameworkElement visual, string fileName)
        {
            var encoder = new PngBitmapEncoder();
            EncodeVisual(visual, fileName, encoder);
        }

        private static void EncodeVisual(FrameworkElement visual, string fileName, BitmapEncoder encoder)
        {
            var bitmap = new RenderTargetBitmap((int)visual.ActualWidth + 200, (int)visual.ActualHeight + 200, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            var frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);
            using (var stream = File.Create(fileName)) encoder.Save(stream);
        }
    }

    public class HeaderFooter : PdfPageEventHelper
    {
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            PdfPTable headerTable = new PdfPTable(2);
            headerTable.SetTotalWidth(new float[] { 400f, 400f });
            headerTable.HorizontalAlignment = Element.ALIGN_RIGHT;

            BaseColor baseColor = new BaseColor(39, 43, 52);

            string strHeader = "Report";
            BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font fntHead = new Font(bfntHead, 16, 1, baseColor);
            Paragraph headingContent = new Paragraph
            {
                Alignment = Element.ALIGN_RIGHT
            };
            headingContent.Add(new Chunk(strHeader.ToUpper(), fntHead));

            PdfPCell leftCell = new PdfPCell(headingContent);
            leftCell.Border = 0;
            leftCell.PaddingTop = 30;
            leftCell.PaddingLeft = 200f;

            iTextSharp.text.Image pngImage = iTextSharp.text.Image.GetInstance("C:/EisaiAtrLogoReports.png");
            //resize image upon your need
            pngImage.ScaleToFit(150f, 150f);
            pngImage.Alignment = Element.ALIGN_RIGHT;

            PdfPCell rightCell = new PdfPCell(pngImage);
            rightCell.Border = 0;
            //rightCell.PaddingLeft = 10;
            headerTable.AddCell(leftCell);
            headerTable.AddCell(rightCell);
            headerTable.WriteSelectedRows(0, -1, document.Left, document.Top + ((document.TopMargin + headerTable.TotalHeight) / 2), writer.DirectContent);
        }
    }
}
