using System.Runtime.Serialization;

namespace ProgrammaticExportSampleAppMPN.Helpers
{
    [DataContract]
    public class ScheduledQueriesObject
    {
        [DataMember(Name = "queryId")]
        public string QueryId { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "description")]
        public string Description { get; set; }
        [DataMember(Name = "query")]
        public string Query { get; set; }
        [DataMember(Name = "type")]
        public string Type { get; set; }
        [DataMember(Name = "user")]
        public string User { get; set; }
        [DataMember(Name = "createdTime")]
        public string CreatedTime { get; set; }
        [DataMember(Name = "modifiedTime")]
        public string ModifiedTime { get; set; }
    }
}
