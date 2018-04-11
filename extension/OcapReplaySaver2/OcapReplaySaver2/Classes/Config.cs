using Newtonsoft.Json;

namespace OcapReplaySaver2
{
    public class Config
    {
        /* Public properties */
        [JsonProperty(PropertyName = "addFileUrl")]
        public string AddURL { get; set; }

        [JsonProperty(PropertyName = "dbInsertUrl")]
        public string InsertURL { get; set; }

        [JsonProperty(PropertyName = "fileDestination")]
        public string FileDestination { get; set; }

        [JsonProperty(PropertyName = "copyLocal")]
        public bool CopyLocal { get; set; }

        [JsonProperty(PropertyName = "httpRequestTimeout")]
        public int Timeout { get; set; }
    }
}
