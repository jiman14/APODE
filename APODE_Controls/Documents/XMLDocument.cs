#region Copyright © 2015, 2016 MJM [info.apode@gmail.com]
/*
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.

* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
* GNU General Public License for more details.

* You should have received a copy of the GNU General Public License
* along with this program.If not, see<http:* www.gnu.org/licenses/>.
*/
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
#endregion