using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace E3.ReactorManager.ReportsManager.Model.Implementations
{
    public class HeaderFooter : PdfPageEventHelper
    {
        public string ReportHeader { get; set; }
        public string ReportLogoPath { get; set; }

        public HeaderFooter(string reportHeader = null, string reportLogoPath = null)
        {
            ReportHeader = reportHeader ?? "REPORT";
            ReportLogoPath = reportLogoPath;
        }

        public override void OnStartPage(PdfWriter writer, Document document)
        {
            PdfPTable headerTable = new PdfPTable(1);
            headerTable.SetTotalWidth(new float[] { 600f });
            headerTable.HorizontalAlignment = Element.ALIGN_CENTER;

            PdfPTable footerTable = new PdfPTable(2);
            footerTable.SetTotalWidth(new float[] { 400f, 200f });
            footerTable.HorizontalAlignment = Element.ALIGN_CENTER;

            BaseColor baseColor = new BaseColor(39, 43, 52);
            BaseFont bfntHead = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font fntHead = new Font(bfntHead, 16, 1, baseColor);

            PdfPCell leftCell = new PdfPCell
            {
                Border = 0,
                PaddingLeft = document.LeftMargin,
            };
            Paragraph leftCellContent = new Paragraph
            {
                Alignment = Element.ALIGN_LEFT,
            };
            leftCellContent.Add(new Chunk($"{writer.PageNumber}", new Font(bfntHead, 13, 0, baseColor)));
            leftCell.AddElement(leftCellContent);

            PdfPCell headerCell = new PdfPCell
            {
                Border = 0,
                Padding = 5,
            };

            Paragraph headingContent = new Paragraph
            {
                Alignment = Element.ALIGN_CENTER,
            };
            headingContent.Add(new Chunk(ReportHeader, fntHead));
            headerCell.AddElement(headingContent);

            PdfPCell rightCell = new PdfPCell
            {
                Border = 0,
                PaddingRight = document.RightMargin
            };

            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + @"Images\e3tech_logo.png");
            if (File.Exists(ReportLogoPath))
            {
                //Add the icon to the Header if exists
                Image rightCellImage = Image.GetInstance(ReportLogoPath);
                //resize image upon your need
                rightCellImage.ScaleToFit(100f, 100f);
                rightCellImage.Alignment = Element.ALIGN_RIGHT;

                //add right cell image to the right cell
                rightCell.AddElement(rightCellImage);
            }
            //Check if client logo exists in the Folder
            else if (File.Exists(imagePath))
            {
                //Add the icon to the Header if exists
                Image rightCellImage = Image.GetInstance(imagePath);
                //resize image upon your need
                rightCellImage.ScaleToFit(100f, 100f);
                rightCellImage.Alignment = Element.ALIGN_RIGHT;

                //add right cell image to the right cell
                rightCell.AddElement(rightCellImage);
            }

            headerTable.AddCell(headerCell);

            footerTable.AddCell(leftCell);
            footerTable.AddCell(rightCell);

            headerTable.WriteSelectedRows(0, -1, 0, document.Top + ((document.TopMargin + headerTable.TotalHeight) / 2), writer.DirectContent);
            footerTable.WriteSelectedRows(0, -1, 0, document.Bottom, writer.DirectContent);
        }
    }
}
