using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ProgrammaticExportSampleAppMPN.Helpers
{
    public class ExportDownloader
    {
        private readonly IConfiguration _configuration;
        private TokenGenerator _tokenGenerator;
        private ILogger<ExportDownloader> _logger;

        public class DownloadArgs
        {
            public string reportId;
            public IConfiguration configuration;
            public TokenGenerator tokenGenerator;
            public ILogger logger;
        }

        public ExportDownloader(IConfiguration configuration, TokenGenerator tokenGenerator, ILogger<ExportDownloader> logger)
        {
            _configuration = configuration;
            _tokenGenerator = tokenGenerator;
            _logger = logger;
            ThreadPool.SetMaxThreads(5, 5);
        }

        public void DownloadReport(string reportId)
        {
            DownloadArgs downloadArgs = new DownloadArgs
            {
                reportId = reportId,
                configuration = _configuration,
                tokenGenerator = _tokenGenerator,
                logger = _logger
            };

            ThreadPool.QueueUserWorkItem(new WaitCallback(DownloadReportThread), downloadArgs);
        }

        public static async void DownloadReportThread(Object obj)
        {
            DownloadArgs download = obj as DownloadArgs;
            download.logger.LogInformation($"Starting download of report {download.reportId}");
            var accessToken = await download.tokenGenerator.GenerateADTokenWithRetries();
            if (string.IsNullOrEmpty(accessToken))
            {
                download.logger.LogError("Token generation failed");
                return;
            }

            var baseUrl = download.configuration["ProgrammaticExport:ApiEndpointUrl"];
            var reportExecutionPath = download.configuration["ProgrammaticExport:ReportExecutionPath"];
            var finalExecutionUriPath = String.Format(reportExecutionPath, download.reportId);
            using var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync(finalExecutionUriPath);
            if ((int)response.StatusCode != 200)
            {
                download.logger.LogError($"Failed to get valid response; {response.StatusCode}");
                return;
            }
            var resp = await response.Content.ReadAsStringAsync();
            APIOutput<ScheduledReportExecutionObject> executionResultObject = JsonConvert.DeserializeObject<APIOutput<ScheduledReportExecutionObject>>(resp);
            if (executionResultObject.TotalCount != 1 || string.IsNullOrEmpty(executionResultObject.Value.First().ReportLocation))
            {
                download.logger.LogError($"Invalid record obtained");
                return;
            }
            try
            {
                UncompressedWebClient webClient = new UncompressedWebClient();
                var reportDir = System.IO.Directory.GetCurrentDirectory() + $"\\DownloadedReports";
                System.IO.Directory.CreateDirectory(reportDir);
                var reportFile = $"{reportDir}\\report_{DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()}.csv";
                webClient.DownloadFile(executionResultObject.Value.First().ReportAccessSecureLink, reportFile);
            } catch (Exception e)
            {
                download.logger.LogError($"Download failed: {e.Message}");
            }
        }
    }

    public class UncompressedWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            return request;
        }
    }
}
