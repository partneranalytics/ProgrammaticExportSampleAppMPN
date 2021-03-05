using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProgrammaticExportSampleAppMPN.Helpers
{
    public class ScheduledDatasetObject
    {
        [DataMember(Name = "datasetName")]
        public string DatasetName { get; set; }
        [DataMember(Name = "selectableColumns")]
        public List<string> SelectableColumns { get; set; }
        [DataMember(Name = "availableMetrics")]
        public List<string> AvailableMetrics { get; set; }
        [DataMember(Name = "availableDateRanges")]
        public List<string> AvailableDateRanges { get; set; }
    }
}
