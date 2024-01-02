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


namespace webapi_csharp.Controllers
{
    [ApiController]
    [Route("/api/db/tables/plans")]
    [EnableCors("AllowSpecificOrigin")]
    public class PlansController : Controller
    {
        private readonly ILogger<PlansController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public PlansController(ILogger<PlansController> logger)
        {
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                              "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        

        

        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> addPlan([FromBody] PlanRequest request) {
            try {
                conn.Open();
                _logger.LogInformation("Successfully connected to PostgreSQL.");

                var query = @"INSERT INTO plans (name, service_id)
                              VALUES (@Name, @ServiceId);
                              SELECT SCOPE_IDENTITY();";

                var id = await conn.QueryFirstOrDefaultAsync<int>(query, request);

                return Ok(new { success = true, message = "Data successfully added to the database.", data = new { id } });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("get/plans_quota_limit/plan_id")]
        [HttpGet]
        public async Task<IActionResult> getPlansQuotaLimitByPlanId([FromQuery] int plan_id) {
            try {
                conn.Open();

                var query = "SELECT * FROM quota_limit WHERE plan_id = @PlanId";

                var rows = await conn.QueryAsync<QuotaLimit>(query, new { PlanId = plan_id });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("get/plans_rate_limit/plan_id")]
        [HttpGet]
        public async Task<IActionResult> getPlansRateLimitByPlanId([FromQuery] int plan_id) {
            try {
                conn.Open();

                var query = "SELECT * FROM rate_limit WHERE plan_id = @PlanId";

                var rows = await conn.QueryAsync<RateLimit>(query, new { PlanId = plan_id });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("get/service_id")]
        [HttpGet]
        public async Task<IActionResult> getPlansByServiceId([FromQuery] int service_id) {
            try {
                conn.Open();

                var query = "SELECT * FROM plans WHERE service_id = @ServiceId";

                var rows = await conn.QueryAsync<Plan>(query, new { VendorId = service_id });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("activate")]
        [HttpPost]
        public async Task<IActionResult> activatePlan([FromBody] ActivatePlanRequest request) {
            try {
                conn.Open();

                var query = "UPDATE plans SET activated = @Activated OUTPUT INSERTED.* WHERE id = @Id;";

                var row = await conn.QueryFirstOrDefaultAsync<Plan>(query, request);

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully updated to the database.", data = row });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("update/plan")]
        [HttpPost]
        public async Task<IActionResult> updatePlan([FromBody] UpdatePlanRequest request) {
            try {
                conn.Open();

                var query = @"UPDATE plans 
                              SET price = @Price, updated_at = GETDATE() OUTPUT INSERTED.* 
                              WHERE id = @Id;";

                var row = await conn.QueryFirstOrDefaultAsync<Plan>(query, request);

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully updated to the database.", data = row });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("update/plan_quota_limit")]
        [HttpPost]
        public async Task<IActionResult> updatePlanQuotaLimit([FromBody] UpdatePlanQuotaLimitRequest request) {
            try {
                conn.Open();

                var query = @"UPDATE quota_limit 
                             SET interval = @Interval, limit_value = @LimitValue, allow_overage = @AllowOverage, overage_fee = @OverageFee OUTPUT INSERTED.* 
                             WHERE id = @Id;";

                var row = await conn.QueryFirstOrDefaultAsync<QuotaLimit>(query, request);

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully updated to the database.", data = row });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("update/plan_rate_limit")]
        [HttpPost]
        public async Task<IActionResult> updatePlanRateLimit([FromBody] UpdatePlanRateLimitRequest request) {
            try {
                conn.Open();

                var query = @"UPDATE rate_limit 
                              SET interval = @Interval, limit_value = @LimitValue OUTPUT INSERTED.* 
                              WHERE id = @Id;";

                var row = await conn.QueryFirstOrDefaultAsync<RateLimit>(query, request);

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully updated to the database.", data = row });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }
    }
    public class PlanRequest
    {
        public string Name { get; set; }
        public int ServiceId { get; set; }
    }

    public class ActivatePlanRequest
    {
        public int Id { get; set; }
        public bool Activated { get; set; }
    }

    public class UpdatePlanRequest
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
    }

    public class UpdatePlanQuotaLimitRequest
    {
        public int Id { get; set; }
        public string Interval { get; set; }
        public int LimitValue { get; set; }
        public bool AllowOverage { get; set; }
        public decimal OverageFee { get; set; }
    }

    public class UpdatePlanRateLimitRequest
    {
        public int Id { get; set; }
        public string Interval { get; set; }
        public int LimitValue { get; set; }
    }

    public class Plan
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ServiceId { get; set; }
        public bool Activated { get; set; }
        public decimal Price { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class QuotaLimit
    {
        public int Id { get; set; }
        public string Interval { get; set; }
        public int LimitValue { get; set; }
        public bool AllowOverage { get; set; }
        public decimal OverageFee { get; set; }
        public int PlanId { get; set; }
    }

    public class RateLimit
    {
        public int Id { get; set; }
        public string Interval { get; set; }
        public int LimitValue { get; set; }
        public int PlanId { get; set; }
    }
}