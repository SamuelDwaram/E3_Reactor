using E3.ReactorManager.Interfaces.DataAbstractionLayer;
using E3.ReactorManager.ReportsManager.Model.Data;
using E3.ReactorManager.ReportsManager.Model.Interfaces;
using E3Tech.IO.FileAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace E3.ReactorManager.ReportsManager.Model.Implementations
{
    public class DocumentHandler : IDocumentHandler
    {
        IFileBrowser fileBrowser = new DefaultFileBrowser();
        IDatabaseReader databaseReader;
        IDatabaseWriter databaseWriter;

        public DocumentHandler(IDatabaseReader databaseReader, IDatabaseWriter databaseWriter)
        {
            this.databaseWriter = databaseWriter;
            this.databaseReader = databaseReader;
        }

        public List<Document> GetAvailableDocuments()
        {
            var pdfList = databaseReader.GetAvailablePdfList();

            if (pdfList.Rows.Count > 0)
            {
                return (from DataRow dr in pdfList.AsEnumerable()
                        select new Document
                        {
                            Identifier = dr["Identifier"].ToString(),
                            Name = dr["Name"].ToString(),
                        }).ToList();
            }

            return new List<Document>();
        }

        public Document OpenDocument(string identifier)
        {
            var DocumentTable = databaseReader.GetDocumentDetails(identifier);

            if (DocumentTable.Rows.Count > 0)
            {
                return (from DataRow dr in DocumentTable.AsEnumerable()
                        select new Document
                        {
                            Identifier = dr["Identifier"].ToString(),
                            Name = dr["Name"].ToString(),
                            Content = (byte[])dr["Content"],
                            ReviewDate = (DateTime)dr["ReviewDate"],
                        }).First();
            }

            return new Document();
        }

        public void UploadDocument(string docExtension)
        {
            string filePath = fileBrowser.OpenFile(docExtension);
            string fileName = Path.GetFileName(filePath);

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                FileStream fs = new FileStream(filePath, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);
                Byte[] bytes = br.ReadBytes((Int32)fs.Length);

                databaseWriter.UploadPdf(fileName, bytes);
            }
        }
    }
}
