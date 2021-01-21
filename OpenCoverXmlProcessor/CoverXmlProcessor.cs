using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using OpenCoverXmlProcessor.Model;

namespace OpenCoverXmlProcessor
{
    class CoverXmlProcessor
    {
        private readonly string filePath;
        private readonly XElement root;

        public CoverXmlProcessor(string filePath)
        {
            this.filePath = filePath;
            this.root = XElement.Load(filePath);
        }

        public void Process(JsonData jsonData, string newXml = null)
        {
            XElement modules = root.Element("Modules");
            List<XElement> elementsToRemove = new List<XElement>();
            foreach (XElement module in modules.Elements("Module"))
            {
                foreach (XElement file in module.XPathSelectElements("./Files/File"))
                {
                    string fullPath = file.Attribute("fullPath").Value;
                    string uid = file.Attribute("uid").Value;
                    Diff fileDiff = jsonData.Diffs.SingleOrDefault(f => ActualPath(f.Path) == Path.GetFullPath(fullPath));
                    foreach (XElement method in module.XPathSelectElements("./Classes/Class/Methods/Method"))
                    {
                        if (method.Element("FileRef").Attribute("uid").Value == uid)
                        {
                            if (fileDiff != null)
                            {
                                // file is to collect coverage but only cover the valid lines
                                foreach (XElement point in method.XPathSelectElements("./SequencePoints/SequencePoint|./BranchPoints/BranchPoint"))
                                {
                                    if (!fileDiff.MatchLine(int.Parse(point.Attribute("uspid").Value)))
                                    {
                                        // remove the unmatched line or branch
                                        elementsToRemove.Add(point);
                                    }
                                }
                            }
                            else
                            {
                                // remove the method that in the file that has no any change
                                elementsToRemove.Add(method);
                            }
                        }
                    }
                }

                Remove(elementsToRemove);
                foreach (XElement classElement in module.XPathSelectElements("./Classes/Class"))
                {
                    if (classElement.XPathSelectElements("./Methods/Method").Count() == 0)
                    {
                        elementsToRemove.Add(classElement);
                    }
                }

                Remove(elementsToRemove);
            }

            Remove(elementsToRemove);

            if (newXml == null)
            {
                root.Save(this.filePath);
            }
            else
            {
                root.Save(newXml);
            }
        }

        private void Remove(List<XElement> elements)
        {
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                try
                {
                    elements.Remove();
                }
                catch (InvalidOperationException)
                {
                    continue;
                }
            }

            elements.Clear();
        }

        private string ActualPath(string relativePath)
        {
            var combinedPath = Path.Combine(ConfigurationManager.AppSettings["RelativeRoot"], relativePath.TrimStart('/'));
            var fullPath = Path.GetFullPath(combinedPath);
            return fullPath;
        }
    }
}
