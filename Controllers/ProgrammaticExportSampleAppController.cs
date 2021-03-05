using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProgrammaticExportSampleAppMPN.Helpers;

namespace ProgrammaticExportSampleAppMPN.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProgrammaticExportSampleAppMPNController : ControllerBase
    {
        private readonly ILogger<ProgrammaticExportSampleAppMPNController> _logger;
        private IConfiguration _configuration;
        private TokenGenerator _tokenGenerator;
        private ExportDownloader _exportDownloader;
        private const string _tokenHeaderKey = "X-AadUserTicket";

        public ProgrammaticExportSampleAppMPNController(
            ILogger<ProgrammaticExportSampleAppMPNController> logger,
            IConfiguration configuration,
            TokenGenerator tokenGenerator,
            ExportDownloader exportDownloader)
        {
            _logger = logger;
            _configuration = configuration;
            _tokenGenerator = tokenGenerator;
            _exportDownloader = exportDownloader;
        }

        [HttpGet]
        [Route("sample")]
        public ContentResult GetSampleApp()
        {
            var htmlText = System.IO.File.ReadAllText("index.html");
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = htmlText
            };
        }

        [HttpGet]
        [Route("datasets")]
        public async Task<IActionResult> GetDatasets()
        {
            var accessToken = await _tokenGenerator.GenerateADTokenWithRetries();
            if (string.IsNullOrEmpty(accessToken))
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Unable to generate AD token");
            }
            var baseUrl = _configuration["ProgrammaticExport:ApiEndpointUrl"];
            var datasetPath = _configuration["ProgrammaticExport:DatasetsPath"];

            using var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(datasetPath);
            if ((int)response.StatusCode != 200)
            {
                return StatusCode((int)response.StatusCode, response);
            }
            var responseString = await response.Content.ReadAsStringAsync();
            APIOutput<ScheduledDatasetObject> datasetResultObject = JsonConvert.DeserializeObject<APIOutput<ScheduledDatasetObject>>(responseString);
            return Ok(datasetResultObject);
        }

        [HttpPost]
        [Route("query")]
        public async Task<IActionResult> CreateQuery(
             [FromQuery(Name = "query")] string query = null,
             [FromQuery(Name = "query_name")] string query_name = null)
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(query_name))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Query name and the query are mandatory");
            }

            var accessToken = await _tokenGenerator.GenerateADTokenWithRetries();
            if (string.IsNullOrEmpty(accessToken))
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Unable to generate AD token");
            }

            // Create the query
            ScheduledQueriesInputObject scheduledQueriesInputObject = new ScheduledQueriesInputObject
            {
                Query = query,
                Name = query_name
            };

            var baseUrl = _configuration["ProgrammaticExport:ApiEndpointUrl"];
            var queryPath = _configuration["ProgrammaticExport:QueryPath"];

            using var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var content = new StringContent(
                JsonConvert.SerializeObject(scheduledQueriesInputObject),
                System.Text.Encoding.UTF8,
                "application/json");

            HttpResponseMessage response = await client.PostAsync(queryPath, content);
            if ((int)response.StatusCode != 200)
            {
                return StatusCode((int)response.StatusCode, response);
            }
            // Read output to get the queryId
            var responseString = await response.Content.ReadAsStringAsync();
            APIOutput<ScheduledQueriesObject> queryResultObject = JsonConvert.DeserializeObject<APIOutput<ScheduledQueriesObject>>(responseString);
            if (queryResultObject.TotalCount != 1)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Query creation failed");
            }

            return Ok(queryResultObject.Value.First().QueryId);
        }

        [HttpGet]
        [Route("query")]
        public async Task<IActionResult> GetQueries()
        {
            var accessToken = await _tokenGenerator.GenerateADTokenWithRetries();
            if (string.IsNullOrEmpty(accessToken))
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Unable to generate AD token");
            }

            var baseUrl = _configuration["ProgrammaticExport:ApiEndpointUrl"];
            var queryPath = _configuration["ProgrammaticExport:QueryPath"];

            using var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync(queryPath);
            if ((int)response.StatusCode != 200)
            {
                return StatusCode((int)response.StatusCode, response);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            APIOutput<ScheduledQueriesObject> queryResultObject = JsonConvert.DeserializeObject<APIOutput<ScheduledQueriesObject>>(responseString);

            return Ok(queryResultObject);
        }

        [HttpGet]
        [Route("tryquery")]
        public async Task<IActionResult> TryQuery(
            [FromQuery(Name = "export_query")] string export_query = null)
        {
            var accessToken = await _tokenGenerator.GenerateADTokenWithRetries();
            if (string.IsNullOrEmpty(accessToken))
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Unable to generate AD token");
            }

            var baseUrl = _configuration["ProgrammaticExport:ApiEndpointUrl"];
            var queryPath = _configuration["ProgrammaticExport:QueryPath"] + $"/testQueryResult?exportQuery={export_query}";

            using var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            HttpResponseMessage response = await client.GetAsync(queryPath);
            if ((int)response.StatusCode != 200)
            {
                return StatusCode((int)response.StatusCode, response);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            return Ok(responseString);
        }

        [HttpPost]
        [Route("report")]
        public async Task<IActionResult> ScheduleReport(
             [FromQuery(Name = "query_id")] string query_id = null,
             [FromQuery(Name = "report_name")] string report_name = null,
             [FromQuery(Name = "report_start_name")] string report_start_time = null,
             [FromQuery(Name = "recurrence_interval")] string recurrence_interval = null,
             [FromQuery(Name = "recurrence_count")] string recurrence_count= null)
        {
            if (string.IsNullOrEmpty(query_id) || string.IsNullOrEmpty(report_name))
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Query Id and the report name are mandatory");
            }

            var accessToken = await _tokenGenerator.GenerateADTokenWithRetries();
            if (string.IsNullOrEmpty(accessToken))
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Unable to generate AD token");
            }

            ScheduledReportCreateInputObject scheduledReportCreateInputObject = new ScheduledReportCreateInputObject()
            {
                QueryId = query_id,
                ReportName = report_name,
                //CallbackUrl should be a publicly accesible URL
                //CallbackUrl = "https://localhost:44365/reportready/",
                RecurrenceInterval = Int32.Parse(string.IsNullOrEmpty(recurrence_interval) ?
                    _configuration["ProgrammaticExport:RecurrenceInterval"] : recurrence_interval),
                RecurrenceCount = Int32.Parse(string.IsNullOrEmpty(recurrence_count) ?
                    _configuration["ProgrammaticExport:RecurrenceCount"] : recurrence_count),
                StartTime = string.IsNullOrEmpty(report_start_time) ?
                    DateTime.UtcNow.AddMinutes(90).ToString("yyyy'-'MM'-'dd HH':'mm':'ss'Z'") : report_start_time
            };

            var baseUrl = _configuration["ProgrammaticExport:ApiEndpointUrl"];
            var reportPath = _configuration["ProgrammaticExport:ReportPath"];
            using var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var content = new StringContent(
                JsonConvert.SerializeObject(scheduledReportCreateInputObject),
                System.Text.Encoding.UTF8,
                "application/json");
            var response = await client.PostAsync(reportPath, content);
            if ((int)response.StatusCode != 200)
            {
                return StatusCode((int)response.StatusCode, response);
            }
            var responseString = await response.Content.ReadAsStringAsync();
            APIOutput<ScheduledReportObject> reportResultObject = JsonConvert.DeserializeObject<APIOutput<ScheduledReportObject>>(responseString);
            if (reportResultObject.TotalCount != 1)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Report creation failed");
            }

            return Ok(reportResultObject.Value.First().ReportId);
        }

        [HttpPost]
        [Route("reportready/{reportId}")]
        public IActionResult ReportReady(string reportId)
        {
            _exportDownloader.DownloadReport(reportId);
            return Ok("Download of report has been queued");
        }
    }
}
