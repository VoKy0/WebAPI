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
    public class SavedServicesController : Controller
    {
        private readonly ILogger<SavedServicesController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public SavedServicesController(ILogger<SavedServicesController> logger)
        {
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                              "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        


        [Route("get/user_id")]
        [HttpGet]
        public async Task<IActionResult> getSavedServiceByUserId([FromQuery] int user_id) {
            try {
                conn.Open();

                var query = "SELECT * FROM saved_services WHERE user_id = @UserId";

                var rows = await conn.QueryAsync<dynamic>(query, new { UserId = user_id });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("get/user_id/service_id")]
        [HttpGet]
        public async Task<IActionResult> getSavedServiceByDetails([FromQuery] int user_id, [FromQuery] int service_id) {
            try {
                conn.Open();

                var query = @"SELECT * 
                              FROM saved_services 
                              WHERE user_id = @UserId AND service_id = @ServiceId";

                var rows = await conn.QueryAsync<dynamic>(query, new { UserId = user_id, ServiceId = service_id });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }
        
        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> addSavedService([FromBody] SavedServicesModel saved_service) {
            try {
                conn.Open();
                _logger.LogInformation("Successfully connected to PostgreSQL.");

                var query = @"INSERT INTO saved_services (user_id, service_id)
                              VALUES (@UserId, @ServiceId);
                              SELECT SCOPE_IDENTITY();";

                var rows = await conn.QueryAsync<int>(query, new { UserId = saved_service.UserId, ServiceId = saved_service.ServiceId });

                return Ok(new { success = true, message = "Data successfully added to the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("delete")]
        [HttpPost]
        public async Task<IActionResult> deleteSavedService([FromBody] SavedServicesModel saved_service) {
            try {
                conn.Open();
                _logger.LogInformation("Successfully connected to PostgreSQL.");

                var query = @"DELETE FROM saved_services
                              WHERE user_id = @UserId AND service_id = @ServiceId";

                var rows = await conn.QueryAsync<dynamic>(query, new { UserId = saved_service.UserId, AppId = saved_service.ServiceId });

                return Ok(new { success = true, message = "Data successfully added to the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }
    }
}