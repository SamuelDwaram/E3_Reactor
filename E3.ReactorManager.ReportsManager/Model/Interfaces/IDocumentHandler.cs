using E3.ReactorManager.ReportsManager.Model.Data;
using System.Collections.Generic;

namespace E3.ReactorManager.ReportsManager.Model.Interfaces
{
    public interface IDocumentHandler
    {
        void UploadDocument(string docExtension);

        Document OpenDocument(string identifier);

        List<Document> GetAvailableDocuments();
    }
}
