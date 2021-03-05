using System;

namespace ProgrammaticExportSampleAppMPN.Helpers
{
    public class ScheduledReportObject
    {
        public string ReportId { get; set; }
        public string ReportName { get; set; }
        public string Description { get; set; }
        public string QueryId { get; set; }
        public string User { get; set; }
        public string CreatedTime { get; set; }
        public string ModifiedTime { get; set; }
        public string StartTime { get; set; }
        public bool IsPublicInOrg { get; set; }
        public string ReportStatus { get; set; }
        public string ExecutionStatus { get; set; }
        public int RecurrenceInterval { get; set; }
        public Nullable<int> RecurrenceCount { get; set; }
        public string CallbackUrl { get; set; }
        public string Format { get; set; }
    }
}
