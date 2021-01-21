using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace OpenCoverXmlProcessor
{
    class Program
    {
        static JsonData jsonData = new JsonData()
        {
            FileDiffs = new FileDiff[]
            {
                new FileDiff
                {
                    Path = @"D:\Projects\Coverage\AppUnderCoverlet\Program.cs",
                    Lines  = new int[][]
                    {
                        new int[]{ 1, 10 }
                    }
                }
            }
        };

        static void Main(string[] args)
        {

            XElement root = XElement.Load("coverage.opencover.xml");
            XElement modules = root.Element("Modules");
            foreach (XElement module in modules.Elements("Module"))
            {
                foreach (XElement file in module.XPathSelectElements("//Files/File"))
                {
                    string fullPath = file.Attribute("fullPath").Value;
                    string uid = file.Attribute("uid").Value;
                    FileDiff fileDiff = jsonData.FileDiffs.SingleOrDefault(f => f.Path == fullPath);
                    if (fileDiff != null)
                    {
                        // file is to collect coverage but only cover the valid lines
                        foreach (XElement method in module.XPathSelectElements("//Classes/Class/Methods/Method"))
                        {
                            if (method.Element("FileRef").Attribute("uid").Value == uid)
                            {
                                List<XElement> elementsToRemove = new List<XElement>();
                                foreach (XElement point in method.XPathSelectElements("//SequencePoints/SequencePoint|//BranchPoints/BranchPoint"))
                                {
                                    if (!fileDiff.MatchLine(int.Parse(point.Attribute("uspid").Value)))
                                    {
                                        // remove the unmatched line or branch
                                        elementsToRemove.Add(point);
                                    }
                                }

                                elementsToRemove.ForEach(e => e.Remove());
                            }
                        }
                    }
                    else
                    {
                        List<XElement> elementsToRemove = new List<XElement>();
                        // don't collect coverage
                        foreach (XElement method in module.XPathSelectElements("//Classes/Class/Methods/Method"))
                        {
                            if (method.Element("FileRef").Attribute("uid").Value == uid)
                            {
                                // remove the method that in the file that has no any change
                                elementsToRemove.Add(method);
                            }
                        }

                        elementsToRemove.ForEach(e => e.Remove());
                    }
                }
            }
            root.Save("test.xml");
            Console.WriteLine(root);
            Console.ReadLine();
        }
    }



    public class JsonData
    {
        public FileDiff[] FileDiffs { get; set; }
    }

    public class FileDiff
    {
        public string Path { get; set; }
        public int[][] Lines { get; set; }

        public bool MatchLine(int lineNumber)
        {
            foreach (var line in Lines)
            {
                return lineNumber >= line[0] && lineNumber <= line[0] + line[1];
            }

            return false;
        }
    }

}
