using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;
using OpenCoverXmlProcessor.Model;

namespace OpenCoverXmlProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            var jsonData = JsonData.Load("data.json");
            CoverXml coverXml = new CoverXml("coverage.opencover.xml");
            coverXml.Process(jsonData, "test.xml");
            Console.ReadLine();
        }
    }
}
