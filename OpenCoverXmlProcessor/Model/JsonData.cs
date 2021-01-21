using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace OpenCoverXmlProcessor.Model
{
    public class JsonData
    {
        [JsonProperty(PropertyName = "diffs")]
        public Diff[] Diffs { get; set; }

        public static JsonData Load(string file)
        {
            string data = File.ReadAllText(file);
            JsonData jsonData = JsonConvert.DeserializeObject<JsonData>(data);
            return jsonData;
        }
    }
}
