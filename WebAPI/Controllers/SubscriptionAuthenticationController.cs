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
using webapi_csharp.Models;

namespace webapi_csharp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SubscriptionAuthenticationController : Controller
    {
        private readonly ILogger<SubscriptionAuthenticationController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public SubscriptionAuthenticationController(ILogger<SubscriptionAuthenticationController> logger)
        {
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                              "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        


        [Route("get/subscription_id")]
        [HttpGet]
        public async Task<IActionResult> getAccessTokenBySubscriptionId([FromQuery] int subscription_id) {
            try {
                conn.Open();

                var query = @"SELECT *
                              FROM subscription_authentication 
                              WHERE subscription_id = @SubscriptionId";

                var rows = await conn.QueryAsync<dynamic>(query, new { SubscriptionId = subscription_id });

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