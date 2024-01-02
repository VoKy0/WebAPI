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
    [Route("/api/db/tables/saved-apps")]
    [EnableCors("AllowSpecificOrigin")]
    public class SavedAppsController : Controller
    {
        private readonly ILogger<SavedAppsController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public SavedAppsController(ILogger<SavedAppsController> logger)
        {
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                              "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        


        [Route("get/user_id")]
        [HttpGet]
        public async Task<IActionResult> getSavedAppsByUserId([FromQuery] int user_id) {
            try {
                conn.Open();

                var query = "SELECT * FROM saved_apps WHERE user_id = @UserId";

                var rows = await conn.QueryAsync<dynamic>(query, new { UserId = user_id });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("get/user_id/app_id")]
        [HttpGet]
        public async Task<IActionResult> getSavedAppByDetails([FromQuery] int user_id, [FromQuery] int app_id) {
            try {
                conn.Open();

                var query = @"SELECT * 
                              FROM saved_apps 
                              WHERE user_id = @UserId AND app_id = @AppId";

                var rows = await conn.QueryAsync<dynamic>(query, new { UserId = user_id, AppId = app_id });

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
        public async Task<IActionResult> addSavedApp([FromBody] SavedApps saved_app) {
            try {
                conn.Open();
                _logger.LogInformation("Successfully connected to PostgreSQL.");

                var query = @"INSERT INTO saved_apps (user_id, app_id)
                              VALUES (@UserId, @AppId);
                              SELECT SCOPE_IDENTITY();";

                var id = await conn.QueryAsync<int>(query, new { UserId = saved_app.UserId, AppId = saved_app.AppId });

                return Ok(new { success = true, message = "Data successfully added to the database.", data = new { id } });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("delete")]
        [HttpPost]
        public async Task<IActionResult> deleteSavedApp([FromBody] SavedApps saved_app) {
            try {
                conn.Open();
                _logger.LogInformation("Successfully connected to PostgreSQL.");

                var query = @"DELETE FROM saved_apps
                              WHERE user_id = @UserId AND app_id = @AppId";

                var rows = await conn.QueryAsync<dynamic>(query, new { UserId = saved_app.UserId, AppId = saved_app.AppId });

                return Ok(new { success = true, message = "Data successfully added to the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }
    }
}