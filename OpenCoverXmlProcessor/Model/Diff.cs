using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace OpenCoverXmlProcessor.Model
{
    public class Diff
    {
        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }

        [JsonProperty(PropertyName = "lines")]
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
