using System.Collections.Generic;

namespace ProgrammaticExportSampleAppMPN.Helpers
{
    public class APIOutput<T>
    {
        public APIOutput()
        {
            this.Value = new List<T>();
        }

        public IEnumerable<T> Value { get; set; }

        public string NextLink { get; set; }

        public long TotalCount { get; set; }

        public string Message { get; set; }

        public int StatusCode { get; set; }

        public bool DataRedacted { get; set; }
    }
}
