using System;

namespace ProgrammaticExportSampleAppMPN.Helpers
{
    public class ScheduledReportExecutionObject
    {
        public string ExecutionId { get; set; }
        public string ReportId { get; set; }
        public int RecurrenceInterval { get; set; }
        public Nullable<int> RecurrenceCount { get; set; }
        public bool IsPublicInOrg { get; set; }
        public string CallbackUrl { get; set; }
        public string Format { get; set; }
        public string ExecutionStatus { get; set; }
        public string ReportLocation { get; set; }
        public string ReportAccessSecureLink { get; set; }
        public string ReportExpiryTime { get; set; }
        public string ReportGeneratedTime { get; set; }
    }
}
