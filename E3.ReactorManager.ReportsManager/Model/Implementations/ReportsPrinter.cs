using System.IO;
using System.Collections.Generic;
using System.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using E3.ReactorManager.ReportsManager.Model.Interfaces;
using E3.ReactorManager.ReportsManager.Model.Data;
using System;
using System.Windows;
using E3Tech.IO.FileAccess;

namespace E3.ReactorManager.ReportsManager.Model.Implementations
{
    public class ReportsPrinter : IReportPrinter
    {
        private readonly IFileBrowser fileBrowser = new DefaultFileBrowser();
        private readonly BaseColor baseColor = new BaseColor(39, 43, 52);

        public event ShowReportPreviewEventHandler ShowReportPreviewEvent;
        public event ClearReportPreviewEventHandler ClearReportPreviewEvent;
        public event ReportGenerationInProgressEventHandler ReportGenerationInProgressEvent;

        public void PrintReportSections(string reportHeader, IList<ReportSection> sections, string reportLogoPath = null)
        {
            string fileName = fileBrowser.SaveFile("Report1", ".pdf");
            while (true)
            {
                if (IsFileLocked(fileName))
                {
                    MessageBox.Show("File is being already used. Please select another file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    fileName = fileBrowser.SaveFile("Report1", ".pdf");
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

                var pdf = CreatePDF(fileName, reportHeader, reportLogoPath);

                /* Open Document for Adding content */
                pdf.Open();

                foreach (ReportSection reportSection in sections)
                {
                    switch (reportSection.DataType)
                    {
                        case SectionalDataType.LabelValuePairs:
                            AddLabelValuePairDataToPdf(reportSection, pdf);
                            break;
                        case SectionalDataType.Tablular:
                            AddTableToPDF(reportSection, pdf);
                            break;
                        //case SectionalDataType.Image:
                        //    AddImageToPdf(reportSection, pdf);
                        //    break;
                        default:
                            break;
                    }
                }

                /* Close Document after Addition of content is Finished */
                pdf.Close();
                ShowReportPreviewEvent?.BeginInvoke(fileName, null, null);
            }
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

        public iTextSharp.text.Document CreatePDF(string fileName, string reportHeader = null, string reportLogoPath = null)
        {
            iTextSharp.text.Document document = new iTextSharp.text.Document(PageSize.A4, 50, 50, 70, 150);
            PdfWriter writer = PdfWriter.GetInstance(document, File.Open(fileName, FileMode.Create));

            //using header class (for adding header in each page)
            writer.PageEvent = new HeaderFooter(reportHeader, reportLogoPath);

            return document;
        }

        public void AddTableToPDF(ReportSection reportSection, iTextSharp.text.Document pdf)
        {
            BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font fntColumnHeader = new Font(btnColumnHeader, 10, 1, BaseColor.BLACK);
            DataTable table = reportSection.Data as DataTable;

            if (table.Rows.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(reportSection.Title))
                {
                    BaseFont bfntHead = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    Font fntHead = new Font(bfntHead, 15, 0, baseColor);

                    Chunk headerChunk = new Chunk(reportSection.Title.ToUpper(), fntHead);
                    Paragraph headerParagraph = new Paragraph(headerChunk)
                    {
                        Alignment = Element.ALIGN_CENTER,
                    };
                    PdfPCell headerCell = new PdfPCell()
                    {
                        BackgroundColor = new BaseColor(153, 153, 153),
                        PaddingBottom = 15
                    };
                    headerCell.AddElement(headerParagraph);
                    PdfPTable headerTable = new PdfPTable(1)
                    {
                        WidthPercentage = 100,
                    };
                    headerTable.AddCell(headerCell);
                    pdf.Add(headerTable);
                }

                //Write the table
                PdfPTable pdfTable = new PdfPTable(table.Columns.Count)
                {
                    HeaderRows = 1  /* Shows table header in every page where the table content is present */
                };
                pdfTable.WidthPercentage = 100;
                //Table Header
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Paragraph columnHeaderContent = new Paragraph(new Chunk(table.Columns[i].ColumnName.ToUpper(), fntColumnHeader)) { Alignment = Element.ALIGN_CENTER };
                    PdfPCell columnHeaderCell = new PdfPCell
                    {
                        BackgroundColor = new BaseColor(217, 217, 217),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        PaddingBottom = 8
                    };
                    columnHeaderCell.AddElement(columnHeaderContent);
                    pdfTable.AddCell(columnHeaderCell);
                }

                //Table Data
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        BaseFont btnColumnValue = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        Font fntColumnValue = new Font(btnColumnValue, 10, 0, baseColor);
                        Paragraph columnContent
                            = new Paragraph(new Chunk(table.Rows[i][j].ToString(), fntColumnValue))
                            {
                                Alignment = Element.ALIGN_CENTER
                            };
                        PdfPCell columnCell = new PdfPCell
                        {
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        };
                        columnCell.AddElement(columnContent);
                        pdfTable.AddCell(columnCell);
                    }
                }

                //#region ValidationFooter
                //PdfPTable tablePDF = new PdfPTable(3);
                //tablePDF.SetTotalWidth(new float[] { 170f, 170f, 170f });
                //tablePDF.HorizontalAlignment = Element.ALIGN_CENTER;
                //tablePDF.SpacingBefore = 50f;

                //PdfPTable columnTable = new PdfPTable(1);

                //string[] data = { "Printed By", "Date : ", "Signature: ", "Verified By", "Date : ", "Signature: ", "Approved By", "Date : ", "Signature: " };
                //int noOfRows = 3;

                //for (int i = 0; i < data.Length; i++)
                //{
                //    if (i != 0 && i % noOfRows == 0)
                //    {
                //        // add columnTable into main table
                //        tablePDF.AddCell(columnTable);

                //        //re initialize columnTable for next column
                //        columnTable = new PdfPTable(1);
                //    }

                //    PdfPCell cell = new PdfPCell(new Paragraph(data[i]));
                //    cell.FixedHeight = 30;
                //    cell.VerticalAlignment = Element.ALIGN_CENTER;

                //    columnTable.AddCell(cell);
                //}

                //// add columnTable for last column into main table
                //tablePDF.AddCell(columnTable);

                //#endregion


                pdf.Add(pdfTable);
                //pdf.Add(tablePDF);

                if (reportSection.EndPageHere)
                {
                    pdf.NewPage();
                }
                else
                {
                    //Add the Line Break to add space between Current Image and Parameters Table
                    pdf.Add(new Paragraph(" "));
                }
            }
        }

        private void AddImageToPdf(ReportSection reportSection, iTextSharp.text.Document pdf)
        {
            if (!string.IsNullOrWhiteSpace(reportSection.Title))
            {
                BaseFont bfntHead = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font fntHead = new Font(bfntHead, 15, 0, baseColor);

                Chunk headerChunk = new Chunk(reportSection.Title.ToUpper(), fntHead);
                Paragraph headerParagraph = new Paragraph(headerChunk)
                {
                    Alignment = Element.ALIGN_CENTER,
                };
                PdfPCell headerCell = new PdfPCell()
                {
                    BackgroundColor = new BaseColor(153, 153, 153),
                    PaddingBottom = 15
                };
                headerCell.AddElement(headerParagraph);
                PdfPTable headerTable = new PdfPTable(1)
                {
                    WidthPercentage = 100,
                };
                headerTable.AddCell(headerCell);
                pdf.Add(headerTable);
            }

            Type dataType = reportSection.Data.GetType();

            if (dataType == typeof(object[]))
            {
                object[] objArray = reportSection.Data as object[];

                //Add Image to the pdf
                string imagePath = Convert.ToString(objArray[0]);
                if (!string.IsNullOrWhiteSpace(imagePath))
                {
                    Image image = Image.GetInstance(imagePath);
                    image.Rotation = (float)Math.PI / 2;
                    image.Alignment = Element.ALIGN_CENTER;
                    pdf.Add(image);
                }
            }
            else if (dataType == typeof(ImageInfo))
            {
                pdf.Add(new Paragraph(""));

                ImageInfo imageInfo = (ImageInfo)reportSection.Data;
                Image image = Image.GetInstance(imageInfo.ImageData);
                image.Alignment = Element.ALIGN_CENTER;
                image.ScaleToFit(480f, 500f);
                pdf.Add(image);

                pdf.Add(new Paragraph(" "));

                BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font fntColumnHeader = new Font(btnColumnHeader, 10, 1, BaseColor.BLACK);
                DataTable table = imageInfo.ParametersData;
                //Write the table
                PdfPTable pdfTable = new PdfPTable(table.Columns.Count)
                {
                    HeaderRows = 1  /* Shows table header in every page where the table content is present */
                };
                pdfTable.WidthPercentage = 100;
                //Table Header
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Paragraph columnHeaderContent = new Paragraph(new Chunk(table.Columns[i].ColumnName.ToUpper(), fntColumnHeader)) { Alignment = Element.ALIGN_CENTER };
                    PdfPCell columnHeaderCell = new PdfPCell
                    {
                        BackgroundColor = new BaseColor(217, 217, 217),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_CENTER,
                        PaddingBottom = 8
                    };
                    columnHeaderCell.AddElement(columnHeaderContent);
                    pdfTable.AddCell(columnHeaderCell);
                }

                //Table Data
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        BaseFont btnColumnValue = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        Font fntColumnValue = new Font(btnColumnValue, 10, 0, baseColor);
                        Paragraph columnContent
                            = new Paragraph(new Chunk(table.Rows[i][j].ToString(), fntColumnValue))
                            {
                                Alignment = Element.ALIGN_CENTER
                            };
                        PdfPCell columnCell = new PdfPCell
                        {
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            VerticalAlignment = Element.ALIGN_MIDDLE
                        };
                        columnCell.AddElement(columnContent);
                        pdfTable.AddCell(columnCell);
                    }
                }

                pdf.Add(pdfTable);
            }

            if (reportSection.EndPageHere)
            {
                pdf.NewPage();
            }
            else
            {
                //Add the Line Break to add space between Current Image and Parameters Table
                pdf.Add(new Paragraph(" "));
            }
        }

        private void AddLabelValuePairDataToPdf(ReportSection reportSection, iTextSharp.text.Document pdf)
        {
            BaseFont baseFontBatchData = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font fontdata = new Font(baseFontBatchData, 13, 1, baseColor);

            if (!string.IsNullOrWhiteSpace(reportSection.Title))
            {
                BaseFont bfntHead = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font fntHead = new Font(bfntHead, 15, 0, baseColor);

                Chunk headerChunk = new Chunk(reportSection.Title.ToUpper(), fntHead);
                Paragraph headerParagraph = new Paragraph(headerChunk)
                {
                    Alignment = Element.ALIGN_CENTER,
                };
                PdfPCell headerCell = new PdfPCell()
                {
                    BackgroundColor = new BaseColor(153, 153, 153),
                    PaddingBottom = 15
                };
                headerCell.AddElement(headerParagraph);
                PdfPTable headerTable = new PdfPTable(1)
                {
                    WidthPercentage = 100,
                };
                headerTable.AddCell(headerCell);
                pdf.Add(headerTable);
            }

            PdfPTable pdfTable = new PdfPTable(new float[] { 500f, 500f });
            pdfTable.WidthPercentage = 100;

            foreach (LabelValuePair data in (reportSection.Data as IList<LabelValuePair>))
            {
                PdfPCell leftCell = new PdfPCell() { VerticalAlignment = Element.ALIGN_MIDDLE };
                PdfPCell rightCell = new PdfPCell() { VerticalAlignment = Element.ALIGN_MIDDLE };

                Paragraph leftCellData = new Paragraph(new Chunk(data.Label, fontdata))
                {
                    Alignment = Element.ALIGN_LEFT,
                    IndentationLeft = 30f
                };

                Paragraph rightCellData = new Paragraph(new Chunk(data.Value, fontdata))
                {
                    Alignment = Element.ALIGN_LEFT,
                    IndentationLeft = 25f
                };
                leftCell.AddElement(leftCellData);
                rightCell.AddElement(rightCellData);

                pdfTable.AddCell(leftCell);
                pdfTable.AddCell(rightCell);
            }
            pdf.Add(pdfTable);
            if (reportSection.EndPageHere)
            {
                pdf.NewPage();
            }
            else
            {
                //Add the Line Break to add space between Current Image and Parameters Table
                pdf.Add(new Paragraph(" "));
            }
        }

        public void ClearReportPreview()
        {
            ClearReportPreviewEvent?.BeginInvoke(null, null);
        }
    }
}
