using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Xml;
using System.IO.Packaging;
using System.Text;

namespace ClickNClaim.WebPortal.Tools
{
    public static class HtmlToDocx
    {
        public static void SaveDOCX(Stream fileStream, string BodyText, bool IncludeHTML)
        {
            // use the Open XML namespace for WordprocessingML:
            string WordprocessingML = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

            // create a new package (Open XML document) ...
            Package pkgOutputDoc = null;
            pkgOutputDoc = Package.Open(fileStream, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            // create the start part, set up the nested structure ...
            XmlDocument xmlStartPart = new XmlDocument();
            XmlElement tagDocument = xmlStartPart.CreateElement("w:document", WordprocessingML);
            xmlStartPart.AppendChild(tagDocument);
            XmlElement tagBody = xmlStartPart.CreateElement("w:body", WordprocessingML);
            tagDocument.AppendChild(tagBody);
            if (IncludeHTML)
            {
                string relationshipNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

                XmlElement tagAltChunk = xmlStartPart.CreateElement("w:altChunk", WordprocessingML);
                XmlAttribute RelID = tagAltChunk.Attributes.Append(xmlStartPart.CreateAttribute("r:id", relationshipNamespace));
                RelID.Value = "rId2";
                tagBody.AppendChild(tagAltChunk);
            }
            else
            {
                XmlElement tagParagraph = xmlStartPart.CreateElement("w:p", WordprocessingML);
                tagBody.AppendChild(tagParagraph);
                XmlElement tagRun = xmlStartPart.CreateElement("w:r", WordprocessingML);
                tagParagraph.AppendChild(tagRun);
                XmlElement tagText = xmlStartPart.CreateElement("w:t", WordprocessingML);
                tagRun.AppendChild(tagText);

                // Note nesting of tags for the WordprocessingML document (the "start part") ...
                // w:document contains ...
                //     w:body, which contains ...
                //         w:p (paragraph), which contains ...
                //             w:r (run), which contains ...
                //                 w:t (text)

                // insert text into the start part, as a "Text" node ...
                XmlNode nodeText = xmlStartPart.CreateNode(XmlNodeType.Text, "w:t", WordprocessingML);
                nodeText.Value = BodyText;
                tagText.AppendChild(nodeText);
            }

            // save the main document part (document.xml) ...
            Uri docuri = new Uri("/word/document.xml", UriKind.Relative);
            PackagePart docpartDocumentXML = pkgOutputDoc.CreatePart(docuri, "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml");
            StreamWriter streamStartPart = new StreamWriter(docpartDocumentXML.GetStream(FileMode.Create, FileAccess.Write));
            xmlStartPart.Save(streamStartPart);
            streamStartPart.Close();
            pkgOutputDoc.Flush();

            // create the relationship part
            pkgOutputDoc.CreateRelationship(docuri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument", "rId1");
            pkgOutputDoc.Flush();

            //this part was taken from the HTMLtoWordML project on CodeProject, authored by Paulo Vaz, which borrowed work from Praveen Bonakurthi
            //original project location for Paulo's project is http://www.codeproject.com/KB/aspnet/HTMLtoWordML.aspx, and it contains a link to the article
            //by Praveen Bonakurthi. I didn't want to have a resident template on the server, so it led to this project

            Uri uriBase = new Uri("/word/document.xml", UriKind.Relative);
            PackagePart partDocumentXML = pkgOutputDoc.GetPart(uriBase);

            Uri uri = new Uri("/word/websiteinput.html", UriKind.Relative);

            //creating the html file from the output of the webform
            string html = string.Concat("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\"><html><head><title></title></head><body>", BodyText, "</body></html>");
            byte[] Origem = Encoding.UTF8.GetBytes(html);
            PackagePart altChunkpart = pkgOutputDoc.CreatePart(uri, "text/html");
            using (Stream targetStream = altChunkpart.GetStream())
            {
                targetStream.Write(Origem, 0, Origem.Length);
            }
            Uri relativeAltUri = PackUriHelper.GetRelativeUri(uriBase, uri);

            //create the relationship in the final file
            partDocumentXML.CreateRelationship(relativeAltUri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/aFChunk", "rId2");

            //close the document ...
            pkgOutputDoc.Flush();
        }
    }
}