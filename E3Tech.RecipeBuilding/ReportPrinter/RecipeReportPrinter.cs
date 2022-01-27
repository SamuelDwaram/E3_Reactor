using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using E3.ReactorManager.Interfaces.DesignExperiment.Data;
using E3Tech.IO.FileAccess;
using E3Tech.RecipeBuilding.Model;
using E3Tech.RecipeBuilding.Model.Blocks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using E3Tech.RecipeBuilding.Converters;
using System.Windows.Media.Imaging;

namespace E3Tech.RecipeBuilding.ReportPrinter
{
    public class RecipeReportPrinter : IRecipeReportPrinter
    {
        readonly IFileBrowser fileBrowser = new DefaultFileBrowser();
        BaseColor baseColor = new BaseColor(39, 43, 52);

        public void AddTableToPDF(DataTable table, Document pdf)
        {
            BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font fntColumnHeader = new Font(btnColumnHeader, 10, 1, BaseColor.WHITE);

            if (table.Rows.Count > 0)
            {
                //Write the table
                PdfPTable pdfTable = new PdfPTable(table.Columns.Count)
                {
                    HeaderRows = 1  /* Shows table header in every page where the table content is present */
                };

                //Table Header
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    Paragraph columnHeaderContent = new Paragraph(new Chunk(table.Columns[i].ColumnName.ToUpper(), fntColumnHeader)) { Alignment = Element.ALIGN_CENTER };
                    PdfPCell columnHeaderCell = new PdfPCell
                    {
                        BackgroundColor = baseColor,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    };
                    columnHeaderCell.AddElement(columnHeaderContent);
                    pdfTable.AddCell(columnHeaderCell);
                }

                //Table Data
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        BaseFont btnColumnValue = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        Font fntColumnValue = new Font(btnColumnValue, 10, 1, baseColor);
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
                pdf.NewPage();
            }
        }

        public Document CreatePdf(string fileName)
        {
            Document document = new Document(PageSize.A4, 50, 50, 80, 30);
            PdfWriter writer = PdfWriter.GetInstance(document, File.Open(fileName, FileMode.Create));

            //using header class (for adding header in each page)
            writer.PageEvent = new HeaderFooter();

            return document;
        }

        public Document CreatePdf(string fileName, string reportHeader)
        {
            Document document = new Document(PageSize.A4, 50, 50, 80, 30);
            PdfWriter writer = PdfWriter.GetInstance(document, File.Open(fileName, FileMode.Create));

            //using header class (for adding header in each page)
            writer.PageEvent = new HeaderFooter(reportHeader);

            return document;
        }

        public void PrintReport(IList<DataTable> tables)
        {
            string fileName = fileBrowser.SaveFile("Report1", ".pdf");

            if (fileName != null)
            {
                var pdf = CreatePdf(fileName);

                /* Open Document for Adding content */
                pdf.Open();

                foreach (var table in tables)
                {
                    AddTableToPDF(table, pdf);
                }

                /* Close Document after Addition of content is Finished */
                pdf.Close();
            }
        }
        public void PrintReport(IList<DataTable> tables, Batch experimentData)
        {
            string fileName = fileBrowser.SaveFile("Report1", ".pdf");

            if (fileName != null)
            {
                var pdf = CreatePdf(fileName);

                /* Open Document for Adding content */
                pdf.Open();

                AddBatchDataToPDF(experimentData, pdf);

                foreach (var table in tables)
                {
                    AddTableToPDF(table, pdf);
                }

                /* Close Document after Addition of content is Finished */
                pdf.Close();
            }
        }

        public void PrintReport(IList<RecipeStep> recipe)
        {
            string fileName = fileBrowser.SaveFile("Report1", ".pdf");

            if (fileName != null)
            {
                var pdf = CreatePdf(fileName);

                /* Open Document for Adding content */
                pdf.Open();

                AddRecipeDataToPDF(recipe, pdf);

                /* Close Document after Addition of content is Finished */
                pdf.Close();
            }
        }

        public void AddRecipeDataToPDF(IList<RecipeStep> recipe, Document pdf)
        {
            BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font fntHead = new Font(bfntHead, 14, 1, baseColor);

            Paragraph headerContent = new Paragraph
            {
                Alignment = Element.ALIGN_LEFT,
            };
            headerContent.Add(new Chunk("3. Operations", fntHead));
            headerContent.SpacingAfter = 20;
            pdf.Add(headerContent);

            if (recipe != null)
            {
                /* Add Operations Table Header */
                PdfPTable pdfPTable = new PdfPTable(4)
                {
                    WidthPercentage = 100,
                    HeaderRows = 1,
                };
                pdfPTable.SetWidths(new float[]{ 100f, 100f, 100f, 300f });

                BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font fntColumnHeader = new Font(btnColumnHeader, 12, 1, baseColor);

                for (int columnIndex = 0; columnIndex < 4; columnIndex++)
                {
                    PdfPCell columnHeaderCell = new PdfPCell();
                    Paragraph columnHeaderContent
                            = new Paragraph(new Chunk(GetColumnHeader(columnIndex), fntColumnHeader))
                            {
                                Alignment = Element.ALIGN_CENTER,
                            };
                    columnHeaderCell.AddElement(columnHeaderContent);
                    columnHeaderCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    columnHeaderCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    pdfPTable.AddCell(columnHeaderCell);
                }

                foreach (var (step, stepIndex) in recipe.Select((v, i) => (v, i)))
                {
                    if (step.BlockOne != null || step.BlockTwo != null || step.BlockThree != null || step.BlockFour != null)
                    {
                        if (step.BlockOne != null)
                        {
                            pdfPTable = AddRecipeBlockToPdf(step.BlockOne, stepIndex, pdfPTable);    
                        }

                        if (step.BlockTwo != null)
                        {
                            pdfPTable = AddRecipeBlockToPdf(step.BlockTwo, stepIndex, pdfPTable);
                        }

                        if (step.BlockThree != null)
                        {
                            pdfPTable = AddRecipeBlockToPdf(step.BlockThree, stepIndex, pdfPTable);
                        }

                        if (step.BlockFour != null)
                        {
                            pdfPTable = AddRecipeBlockToPdf(step.BlockFour, stepIndex, pdfPTable);
                        }
                    }
                }

                pdf.Add(pdfPTable);
            }
        }

        private PdfPTable AddRecipeBlockToPdf(IRecipeBlock block, int stepIndex, PdfPTable pdfPTable)
        {
            if (block == null)
                return pdfPTable;
            switch (block.Name)
            {
                case "Start":
                    return AddStartBlockToPdf((block as ParameterizedRecipeBlock<StartBlockParameters>).Parameters, pdfPTable);
                case "HeatCool":
                    return AddHeatCoolBlockToPdf((block as ParameterizedRecipeBlock<HeatCoolBlockParameters>).Parameters, pdfPTable);
                case "Stirrer":
                    return AddStirrerBlockToPdf((block as ParameterizedRecipeBlock<StirrerBlockParameters>).Parameters, pdfPTable);
                case "Dosing":
                    return AddDosingBlockToPdf((block as ParameterizedRecipeBlock<DosingBlockParameters>).Parameters, pdfPTable);
                case "Wait":
                    return AddWaitBlockToPdf((block as ParameterizedRecipeBlock<WaitBlockParameters>).Parameters, pdfPTable);
                case "End":
                    return AddEndBlockToPdf((block as ParameterizedRecipeBlock<EndBlockParameters>).Parameters, pdfPTable);
            }

            return pdfPTable;
        }

        private PdfPTable AddEndBlockToPdf(EndBlockParameters parameters, PdfPTable pdfPTable)
        {
            BaseFont baseFontRecipeData = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font fontRecipeData = new Font(baseFontRecipeData, 10, 1, baseColor);

            if (parameters != null)
            {
                PdfPCell firstCell = new PdfPCell { Border = 0 };
                PdfPCell secondCell = new PdfPCell { Border = 0 };
                PdfPCell thirdCell = new PdfPCell { Border = 0 };
                PdfPCell fourthCell = new PdfPCell { Border = 0 };

                Paragraph firstCellData = new Paragraph(new Chunk(parameters.StartedTime, fontRecipeData))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                firstCell.AddElement(firstCellData);

                Paragraph secondCellData = new Paragraph(new Chunk(parameters.EndedTime, fontRecipeData))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                secondCell.AddElement(secondCellData);

                Paragraph thirdCellData = new Paragraph(new Chunk(GetBlockDuration(parameters.StartedTime, parameters.EndedTime), fontRecipeData))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                thirdCell.AddElement(thirdCellData);

                Paragraph fourthCellData = new Paragraph(new Chunk("Recipe Ended at : " + parameters.EndedTime , fontRecipeData))
                {
                    Alignment = Element.ALIGN_LEFT
                };
                fourthCell.AddElement(fourthCellData);

                pdfPTable.AddCell(firstCell);
                pdfPTable.AddCell(secondCell);
                pdfPTable.AddCell(thirdCell);
                pdfPTable.AddCell(fourthCell);
            }

            return pdfPTable;
        }

        private PdfPTable AddWaitBlockToPdf(WaitBlockParameters parameters, PdfPTable pdfPTable)
        {
            BaseFont baseFontRecipeData = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font fontRecipeData = new Font(baseFontRecipeData, 10, 1, baseColor);

            if (parameters != null)
            {
                PdfPCell firstCell = new PdfPCell { Border = 0 };
                PdfPCell secondCell = new PdfPCell { Border = 0 };
                PdfPCell thirdCell = new PdfPCell { Border = 0 };
                PdfPCell fourthCell = new PdfPCell { Border = 0 };

                Paragraph firstCellData = new Paragraph(new Chunk(parameters.StartedTime, fontRecipeData))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                firstCell.AddElement(firstCellData);

                Paragraph secondCellData = new Paragraph(new Chunk(parameters.EndedTime, fontRecipeData))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                secondCell.AddElement(secondCellData);

                Paragraph thirdCellData = new Paragraph(new Chunk(GetBlockDuration(parameters.StartedTime, parameters.EndedTime), fontRecipeData))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                thirdCell.AddElement(thirdCellData);

                Paragraph fourthCellData = new Paragraph(new Chunk("Wait " + parameters.Duration + " minutes", fontRecipeData))
                {
                    Alignment = Element.ALIGN_LEFT
                };
                fourthCell.AddElement(fourthCellData);

                pdfPTable.AddCell(firstCell);
                pdfPTable.AddCell(secondCell);
                pdfPTable.AddCell(thirdCell);
                pdfPTable.AddCell(fourthCell);
            }

            return pdfPTable;
        }

        private PdfPTable AddDosingBlockToPdf(DosingBlockParameters parameters, PdfPTable pdfPTable)
        {
            BaseFont baseFontRecipeData = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font fontRecipeData = new Font(baseFontRecipeData, 10, 1, baseColor);

            if (parameters != null)
            {
                PdfPCell firstCell = new PdfPCell { Border = 0 };
                PdfPCell secondCell = new PdfPCell { Border = 0 };
                PdfPCell thirdCell = new PdfPCell { Border = 0 };
                PdfPCell fourthCell = new PdfPCell { Border = 0 };

                Paragraph firstCellData = new Paragraph(new Chunk(parameters.StartedTime, fontRecipeData))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                firstCell.AddElement(firstCellData);

                Paragraph secondCellData = new Paragraph(new Chunk(parameters.EndedTime, fontRecipeData))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                secondCell.AddElement(secondCellData);

                Paragraph thirdCellData = new Paragraph(new Chunk(GetBlockDuration(parameters.StartedTime, parameters.EndedTime), fontRecipeData))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                thirdCell.AddElement(thirdCellData);

                Paragraph fourthCellData = new Paragraph(new Chunk("Dosing Started with Set point : " + parameters.SetPoint, fontRecipeData))
                {
                    Alignment = Element.ALIGN_LEFT
                };
                fourthCell.AddElement(fourthCellData);

                pdfPTable.AddCell(firstCell);
                pdfPTable.AddCell(secondCell);
                pdfPTable.AddCell(thirdCell);
                pdfPTable.AddCell(fourthCell);
            }

            return pdfPTable;
        }

        private PdfPTable AddStirrerBlockToPdf(StirrerBlockParameters parameters, PdfPTable pdfPTable)
        {
            BaseFont baseFontRecipeData = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font fontRecipeData = new Font(baseFontRecipeData, 10, 1, baseColor);

            if (parameters != null)
            {
                PdfPCell firstCell = new PdfPCell { Border = 0 };
                PdfPCell secondCell = new PdfPCell { Border = 0 };
                PdfPCell thirdCell = new PdfPCell { Border = 0 };
                PdfPCell fourthCell = new PdfPCell { Border = 0 };

                Paragraph firstCellData = new Paragraph(new Chunk(parameters.StartedTime, fontRecipeData))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                firstCell.AddElement(firstCellData);

                Paragraph secondCellData = new Paragraph(new Chunk(parameters.EndedTime, fontRecipeData))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                secondCell.AddElement(secondCellData);

                Paragraph thirdCellData = new Paragraph(new Chunk(GetBlockDuration(parameters.StartedTime, parameters.EndedTime), fontRecipeData))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                thirdCell.AddElement(thirdCellData);

                Paragraph fourthCellData = new Paragraph(new Chunk("Stirrer Started with Set point : " + parameters.SetPoint, fontRecipeData))
                {
                    Alignment = Element.ALIGN_LEFT
                };
                fourthCell.AddElement(fourthCellData);

                pdfPTable.AddCell(firstCell);
                pdfPTable.AddCell(secondCell);
                pdfPTable.AddCell(thirdCell);
                pdfPTable.AddCell(fourthCell);
            }

            return pdfPTable;
        }

        private PdfPTable AddHeatCoolBlockToPdf(HeatCoolBlockParameters parameters, PdfPTable pdfPTable)
        {
            BaseFont baseFontRecipeData = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font fontRecipeData = new Font(baseFontRecipeData, 10, 1, baseColor);

            if (parameters != null)
            {
                PdfPCell firstCell = new PdfPCell { Border = 0 };
                PdfPCell secondCell = new PdfPCell { Border = 0 };
                PdfPCell thirdCell = new PdfPCell { Border = 0 };
                PdfPCell fourthCell = new PdfPCell { Border = 0 };

                Paragraph firstCellData = new Paragraph(new Chunk(parameters.StartedTime, fontRecipeData))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                firstCell.AddElement(firstCellData);

                Paragraph secondCellData = new Paragraph(new Chunk(parameters.EndedTime, fontRecipeData))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                secondCell.AddElement(secondCellData);

                Paragraph thirdCellData = new Paragraph(new Chunk(GetBlockDuration(parameters.StartedTime, parameters.EndedTime), fontRecipeData))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                thirdCell.AddElement(thirdCellData);

                Paragraph fourthCellData = new Paragraph(new Chunk("HC Started with Set point : " + parameters.SetPoint, fontRecipeData))
                {
                    Alignment = Element.ALIGN_LEFT
                };
                fourthCell.AddElement(fourthCellData);

                pdfPTable.AddCell(firstCell);
                pdfPTable.AddCell(secondCell);
                pdfPTable.AddCell(thirdCell);
                pdfPTable.AddCell(fourthCell);
            }

            return pdfPTable;
        }

        private PdfPTable AddStartBlockToPdf(StartBlockParameters parameters, PdfPTable pdfPTable)
        {
            BaseFont baseFontRecipeData = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font fontRecipeData = new Font(baseFontRecipeData, 10, 1, baseColor);

            if (parameters != null)
            {
                PdfPCell firstCell = new PdfPCell { Border = 0 };
                PdfPCell secondCell = new PdfPCell { Border = 0 };
                PdfPCell thirdCell = new PdfPCell { Border = 0 };
                PdfPCell fourthCell = new PdfPCell { Border = 0 };

                Paragraph firstCellData = new Paragraph(new Chunk(parameters.StartedTime, fontRecipeData))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                firstCell.AddElement(firstCellData);

                Paragraph secondCellData = new Paragraph(new Chunk(parameters.EndedTime, fontRecipeData))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                secondCell.AddElement(secondCellData);

                Paragraph thirdCellData = new Paragraph(new Chunk(GetBlockDuration(parameters.StartedTime, parameters.EndedTime), fontRecipeData))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                thirdCell.AddElement(thirdCellData);

                Paragraph fourthCellData = new Paragraph(new Chunk("Started Recipe in : " + parameters.HeatCoolModeSelection, fontRecipeData))
                {
                    Alignment = Element.ALIGN_LEFT
                };
                fourthCell.AddElement(fourthCellData);

                pdfPTable.AddCell(firstCell);
                pdfPTable.AddCell(secondCell);
                pdfPTable.AddCell(thirdCell);
                pdfPTable.AddCell(fourthCell);
            }

            return pdfPTable;
        }

        private string GetBlockDuration(string startedTime, string endedTime)
        {
            if (string.IsNullOrWhiteSpace(startedTime) || string.IsNullOrWhiteSpace(endedTime))
            {
                return string.Empty;
            }
            var diff = DateTime.Parse(endedTime) - DateTime.Parse(startedTime);

            return diff.Hours + ":" + diff.Minutes + ":" + diff.Seconds;
        }

        private string GetColumnHeader(int columnIndex)
        {
            switch (columnIndex)
            {
                case 0:
                    return "Started";
                case 1:
                    return "Completed";
                case 2:
                    return "Duration";
                case 3:
                    return "Action Performed";
                default:
                    return "";
            }
        }

        public void AddBatchDataToPDF(Batch experimentData, Document pdf)
        {
            BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font fntHead = new Font(bfntHead, 14, 1, baseColor);

            Paragraph headerContent = new Paragraph
            {
                Alignment = Element.ALIGN_LEFT,
            };
            headerContent.Add(new Chunk("1. Batch Details", fntHead));
            pdf.Add(headerContent);

            if (experimentData != null)
            {
                BaseFont baseFontBatchData = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font fontBatchData = new Font(baseFontBatchData, 13, 1, baseColor);

                Paragraph recipeDataReportHeader = new Paragraph(new Chunk("Recipe Report", new Font(baseFontBatchData, 15, 1, BaseColor.RED)));

                PdfPTable pdfTable = new PdfPTable(3) { };

                var experimentProperties = experimentData.GetType().GetProperties();

                foreach (var eachProperty in experimentProperties)
                {
                    var propertyInfo = experimentData.GetType().GetProperty(eachProperty.Name);
                    var propertyValue = propertyInfo.GetValue(experimentData, null);

                    if (propertyValue != null)
                    {
                        PdfPCell leftCell = new PdfPCell { Border = 0 };
                        PdfPCell middleCell = new PdfPCell { Border = 0 };
                        PdfPCell rightCell = new PdfPCell { Border = 0 };

                        Paragraph leftCellData = new Paragraph(new Chunk(eachProperty.Name, fontBatchData))
                        {
                            Alignment = Element.ALIGN_LEFT
                        };

                        Paragraph middleCellData = new Paragraph(new Chunk(":", fontBatchData))
                        {
                            Alignment = Element.ALIGN_CENTER
                        };

                        Paragraph rightCellData = new Paragraph(new Chunk(propertyValue.ToString(), fontBatchData))
                        {
                            Alignment = Element.ALIGN_LEFT
                        };
                        leftCell.AddElement(leftCellData);
                        middleCell.AddElement(middleCellData);
                        rightCell.AddElement(rightCellData);

                        pdfTable.AddCell(leftCell);
                        pdfTable.AddCell(middleCell);
                        pdfTable.AddCell(rightCell);
                    }
                }
                pdf.Add(pdfTable);
                pdf.NewPage();
            }
        }

        public void PrintReport(IList<RecipeStep> recipe, Batch batch)
        {
            string fileName = fileBrowser.SaveFile("Report1", ".pdf");

            if (fileName != null)
            {
                var pdf = CreatePdf(fileName, batch.Name + " " + batch.Number);

                /* Open Document for Adding content */
                pdf.Open();

                AddBatchDataToPDF(batch, pdf);

                pdf.NewPage();

                AddRecipeDataToPDF(recipe, pdf);

                /* Close Document after Addition of content is Finished */
                pdf.Close();
            }
        }

        private void AddRecipeScreenshotToPdf(RenderTargetBitmap renderTargetBitmap, Document pdf)
        {
            if (renderTargetBitmap != null)
            {
                var bitmapImage = new BitmapImage();
                var bitmapEncoder = new PngBitmapEncoder();
                bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                using (var stream = new MemoryStream())
                {
                    /* Add Header to image content */
                    BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    Font fntHead = new Font(bfntHead, 14, 1, baseColor);

                    Paragraph headerContent = new Paragraph
                    {
                        Alignment = Element.ALIGN_LEFT,
                    };
                    headerContent.Add(new Chunk("2. Phase Grid", fntHead));
                    pdf.Add(headerContent);

                    bitmapEncoder.Save(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();

                    Image recipeScreenshot = Image.GetInstance(stream);
                    recipeScreenshot.ScaleToFit(150f, 150f);
                    recipeScreenshot.Alignment = Element.ALIGN_RIGHT;

                    pdf.Add(recipeScreenshot);
                }
            }
        }

        public void PrintReport(IList<RecipeStep> recipe, Batch batch, RenderTargetBitmap renderTargetBitmap)
        {
            string fileName = fileBrowser.SaveFile("Report1", ".pdf");

            if (fileName != null)
            {
                var pdf = CreatePdf(fileName, batch.Name + " " + batch.Number);

                /* Open Document for Adding content */
                pdf.Open();

                AddBatchDataToPDF(batch, pdf);

                pdf.NewPage();

                AddRecipeScreenshotToPdf(renderTargetBitmap, pdf);

                pdf.NewPage();

                AddRecipeDataToPDF(recipe, pdf);

                /* Close Document after Addition of content is Finished */
                pdf.Close();
            }
        }
    }

    public class HeaderFooter : PdfPageEventHelper
    {
        public string ReportHeader { get; set; }

        public HeaderFooter(string reportHeader)
        {
            ReportHeader = reportHeader;
        }

        public HeaderFooter()
        {

        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            PdfPTable headerTable = new PdfPTable(2);
            headerTable.SetTotalWidth(new float[] { 400f, 200f });
            headerTable.HorizontalAlignment = Element.ALIGN_CENTER;

            BaseColor baseColor = new BaseColor(39, 43, 52);

            string strHeader = string.IsNullOrWhiteSpace(ReportHeader) ? "Report" : ReportHeader;
            BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font fntHead = new Font(bfntHead, 16, 1, baseColor);

            //get the path to Desktop for saving the images
            string imagePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            imagePath += "/Resources";
            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }

            PdfPCell headerCell = new PdfPCell
            {
                Border = 0,
                PaddingLeft = 40,
                PaddingTop = 20,
            };

            Paragraph headerContent = new Paragraph
            {
                Alignment = Element.ALIGN_LEFT,
            };
            headerContent.Add(new Chunk(strHeader.ToUpper(), fntHead));

            headerCell.AddElement(headerContent);
            PdfPCell reportIconCell = new PdfPCell
            {
                Border = 0,
            };

            if (File.Exists(imagePath + "/EisaiAtrLogoReports.png"))
            {
                Image reportIconImage = Image.GetInstance(imagePath + "/EisaiAtrLogoReports.png");
                reportIconImage.ScaleToFit(150f, 150f);
                reportIconImage.Alignment = Element.ALIGN_RIGHT;

                reportIconCell.AddElement(reportIconImage);

            }
            
            headerTable.AddCell(headerCell);
            headerTable.AddCell(reportIconCell);
            headerTable.WriteSelectedRows(0, -1, 0, document.Top + ((document.TopMargin + headerTable.TotalHeight) / 2), writer.DirectContent);
        }
    }
}
