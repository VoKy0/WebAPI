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
using Microsoft.AspNetCore.Cors;

using webapi_csharp.Models;

namespace webapi_csharp.Controllers
{
    [ApiController]
    [Route("/api/db/local-analytics")]
    [EnableCors("AllowSpecificOrigin")]

    public class LocalAnalyticsController : Controller
    {
        private readonly ILogger<LocalAnalyticsController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public LocalAnalyticsController(ILogger<LocalAnalyticsController> logger)
        {
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                       "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        



        
        [Route("get/customer-count/service_id/start_date/end_date")]
        [HttpGet]
        public async Task<IActionResult> getCustomerCountWithinTimeRangeByServiceId([FromQuery] int service_id, [FromQuery] DateTime start_date, [FromQuery] DateTime end_date) {
            try {
                conn.Open();

                var query = @"SELECT service_id, COUNT(DISTINCT user_id) AS customer_count
                              FROM subscriptions
                              WHERE DATE(created_at) BETWEEN DATE(@StartDate) AND DATE(@EndDate)
                                    AND service_id = @ServiceId
                              GROUP BY service_id";

                var rows = await conn.QueryAsync<int>(query, new { @StartDate = start_date, @EndDate = end_date, @ServiceId = service_id });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("get/request-count/service_id/start_date/end_date")]
        [HttpGet]
        public async Task<IActionResult> getRequestCountWithinTimeRangeByServiceId([FromQuery] int service_id, [FromQuery] DateTime start_date, [FromQuery] DateTime end_date) {
            try {
                conn.Open();

                var rows = await Requests.getRequestCountWithinTimeRangeByServiceId(conn, service_id, start_date, end_date);

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("get/avg-request-latency/service_id/start_date/end_date")]
        [HttpGet]
        public async Task<IActionResult> getAvgRequestLatencyWithinTimeRangeByServiceId([FromQuery] int service_id, [FromQuery] DateTime start_date, [FromQuery] DateTime end_date) {
            try {
                conn.Open();

                var rows = await Requests.getAvgRequestLatencyWithinTimeRangeByServiceId(conn, service_id, start_date, end_date);

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("get/total-revenue/service_id/month/year")]
        [HttpGet]
        public async Task<IActionResult> getTotalMonthRevenueByServiceId([FromQuery] int service_id, [FromQuery] int month, [FromQuery] int year) {
            try {
                conn.Open();

                var query = @"SELECT service_id, SUM(amount) AS total_revenue
                              FROM subscriptions
                              WHERE service_id = @ServiceId AND
                                    EXTRACT(MONTH FROM created_at) = @Month AND
                                    EXTRACT(YEAR FROM created_at) = @Year
                              GROUP BY service_id";

                var rows = await conn.QueryAsync<dynamic>(query, new { ServiceId = service_id, Month = month, Year = year});

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("get/avg-revenue/service_id/start_date/end_date")]
        [HttpGet]
        public async Task<IActionResult> getAvgRevenueWithinTimeRangeByServiceId([FromQuery] int service_id, [FromQuery] DateTime start_date, [FromQuery] DateTime end_date) {
            try {
                conn.Open();

                var query = @"SELECT service_id, AVG(amount) AS avg_revenue
                              FROM subscriptions
                              WHERE service_id = @ServiceId AND
                                    DATE(created_at) >= @StartDate AND
                                    DATE(created_at) <= @EndDate AND
                              GROUP BY service_id";

                var rows = await conn.QueryAsync<dynamic>(query, new { ServiceId = service_id, StartDate = start_date, EndDate = end_date });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("get/top-users/user_id/service_id/limit")]
        [HttpGet]
        public async Task<IActionResult> getTopUsersOnRequestCount([FromQuery] int service_id, [FromQuery] int limit) {
            try {
                conn.Open();

                var query = @"SELECT u.*, COUNT(R.id) AS request_count, SUM(S.amount) AS total_revenue
                              FROM subscriptions S, users U
                                   LEFT JOIN requests R ON R.subscription_id = S.id
                              WHERE U.id = S.user_id AND S.service_id = @ServiceId
                              GROUP BY U.id
                              ORDER BY request_count DESC, total_revenue DESC
                              LIMIT @Limit";

                var rows = await conn.QueryAsync<dynamic>(query, new { ServiceId = service_id, Limit = limit });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }
    }
}
