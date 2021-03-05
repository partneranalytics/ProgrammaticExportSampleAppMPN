namespace ProgrammaticExportSampleAppMPN.Helpers
{
    public class ScheduledReportCreateInputObject
    {
        public string ReportName { get; set; }
        public string Description { get; set; }
        public string QueryId { get; set; }
        public string StartTime { get; set; }
        public int RecurrenceInterval { get; set; }
        public int RecurrenceCount { get; set; }
        public string Format { get; set; }
        public string CallbackUrl { get; set; }
    }
}
