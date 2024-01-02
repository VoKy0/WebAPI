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
    [Route("/api/db/tables/subscriptions")]
    [EnableCors("AllowSpecificOrigin")]
    public class SubscriptionsController : Controller
    {
        private readonly ILogger<SubscriptionsController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public SubscriptionsController(ILogger<SubscriptionsController> logger)
        {
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                              "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        


        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> addSubscription([FromBody] SubscriptionsModel subscription) {
            try {
                conn.Open();
                _logger.LogInformation("Successfully connected to PostgreSQL.");

                var query = @"INSERT INTO saved_services (user_id, service_id, plan_id, payment_status)
                              VALUES (@UserId, @ServiceId, @PlanId, @PaymentStatus);
                              RETURNING *";

                var rows = await conn.QueryAsync<dynamic>(query, new { UserId = subscription.UserId, ServiceId = subscription.ServiceId, PlanId = subscription.PlanId, PaymentStatus = subscription.PaymentStatus });

                return Ok(new { success = true, message = "Data successfully added to the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("get/id")]
        [HttpGet]
        public async Task<IActionResult> getSubscriptionById([FromQuery] int id) {
            try {
                conn.Open();

                var query = @"SELECT *
                              FROM subscriptions
                              WHERE id = @Id";

                var rows = await conn.QueryAsync<dynamic>(query, new { Id = id });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("get/user_id")]
        [HttpGet]
        public async Task<IActionResult> getSubscriptionsByUserId([FromQuery] int user_id) {
            try {
                conn.Open();

                var query = @"SELECT *
                              FROM subscriptions
                              WHERE user_id = @UserId
                              ORDER BY start_date DESC";

                var rows = await conn.QueryAsync<dynamic>(query, new { UserId = user_id });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("cancel/user_id/service_id")]
        [HttpPost]
        public async Task<IActionResult> cancelSubscriptionByDetails([FromBody] SubscriptionsModel subscription) {
            try {
                conn.Open();
                _logger.LogInformation("Successfully connected to PostgreSQL.");

                var query = @"UPDATE subscriptions
                              SET status = 'cancelled'
                              WHERE user_id = @UserId AND service_id = @ServiceId AND status = 'active'
                              RETURNING *";

                var rows = await conn.QueryAsync<dynamic>(query, new { UserId = subscription.UserId, ServiceId = subscription.ServiceId });

                return Ok(new { success = true, message = "Data successfully added to the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("update")]
        [HttpPost]
        public async Task<IActionResult> updateSubscription([FromBody] SubscriptionsModel subscription) {
            try {
                conn.Open();
                _logger.LogInformation("Successfully connected to PostgreSQL.");

                var query = @"UPDATE subscriptions
                              SET status = @Status, payment_status = @PaymentStatus, end_date = @EndDate
                              WHERE id = @Id
                              RETURNING *";

                var rows = await conn.QueryAsync<dynamic>(query, new { Id = subscription.Id, Status = subscription.Status, PaymentStatus = subscription.PaymentStatus, EndDate = subscription.EndDate });

                return Ok(new { success = true, message = "Data successfully added to the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("get/user_id/service_id")]
        [HttpGet]
        public async Task<IActionResult> getSubscriptionByDetails([FromQuery] int user_id, [FromQuery] int service_id) {
            try {
                conn.Open();

                var query = @"SELECT *
                              FROM subscriptions
                              WHERE user_id = @UserId AND service_id = @ServiceId AND status = 'active'
                             ";

                var rows = await conn.QueryAsync<dynamic>(query, new { UserId = user_id, ServiceId = service_id });

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