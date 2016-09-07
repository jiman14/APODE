using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;


namespace APODE_Controls
{
    public class OpenXMLDocument
    { 
        public void CreateWordDocument(String file_path, String rtfEncodedString)
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Create(file_path, WordprocessingDocumentType.Document))
            {
                // Create main document, add body and paragraph
                MainDocumentPart mainDocPart = doc.AddMainDocumentPart();
                mainDocPart.Document = new Document();
                Body body = new Body();
                mainDocPart.Document.Append(body);

                String altChunkId = "Id1";
                                
                // Set alternative format RTF to import text
                AlternativeFormatImportPart chunk = mainDocPart.AddAlternativeFormatImportPart(
                    AlternativeFormatImportPartType.Rtf, altChunkId);

                // Convert rtf to chunk data
                using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(rtfEncodedString)))
                {
                    chunk.FeedData(ms);
                }

                AltChunk altChunk = new AltChunk();
                altChunk.Id = altChunkId;

                mainDocPart.Document.Body.InsertAt(
                  altChunk, 0);
               
                // Save changes to the main document part. 
                mainDocPart.Document.Save();

                // Open word compatible aplication
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = file_path;                
                Process.Start(startInfo);                
            }
        }
    }
}
