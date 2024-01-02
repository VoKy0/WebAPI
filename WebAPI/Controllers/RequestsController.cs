using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using Dapper;
using System.Transactions;
using Microsoft.AspNetCore.Cors;

using webapi_csharp.Models;

namespace webapi_csharp.Controllers
{
    [ApiController]
    [Route("/api/db/tables/requests")]
    [EnableCors("AllowSpecificOrigin")]
    public class RequestsController : Controller
    {
        private readonly ILogger<RequestsController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public RequestsController(ILogger<RequestsController> logger)
        {
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                       "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        

        [Route("get-analytics/service_id")]
        [HttpGet]
        public async Task<IActionResult> getRequestAnalyticsByServiceId([FromQuery] int service_id) {
            try {
                conn.Open();
                _logger.LogInformation("Successfully connected to PostgreSQL.");

                var query = @"SELECT 
                                COUNT(R.id) AS request_count,
                                AVG(latency) AS avg_latency
                              FROM
                                requests R, subscriptions S
                              WHERE
                                R.subscription_id = S.id AND
                                S.service_id = @ServiceId
                              GROUP BY 
                                S.service_id";

                var rows = await conn.QueryAsync<dynamic>(query, new { ServiceId = service_id });

                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> addRequest([FromBody] Requests request) {
            try {
                conn.Open();

                var rows = await Requests.create(conn, request.SubscriptionId, request.Latency, request.IsOverage);

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully added to the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }
    }
}