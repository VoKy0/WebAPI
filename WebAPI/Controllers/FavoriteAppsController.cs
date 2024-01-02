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
    [Route("/api/db/tables/favorite-apps")]
    [EnableCors("AllowSpecificOrigin")]
    public class FavoriteAppsController : Controller
    {
        private readonly ILogger<FavoriteAppsController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public FavoriteAppsController(ILogger<FavoriteAppsController> logger)
        {
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                       "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        

        
        [Route("get-count/app_id")]
        [HttpGet]
        public async Task<IActionResult> getFavoriteCountByAppId([FromQuery] int app_id) {
            try {
                conn.Open();

                var query = @"SELECT COUNT(*) AS favorite_count
                              FROM favorite_apps
                              WHERE app_id = @AppId
                              GROUP BY app_id";

                var rows = await conn.QueryAsync<int>(query, new { AppId = app_id });

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
        public async Task<IActionResult> getFavoriteAppByDetails([FromQuery] int user_id, [FromQuery] int app_id) {
            try {
                conn.Open();

                var query = @"SELECT *
                              FROM favorite_apps
                              WHERE user_id = @UserId AND app_id = @AppId";

                var rows = await conn.QueryAsync<object>(query, new { UserId = user_id, AppId = app_id });

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
        public async Task<IActionResult> addFavoriteApp([FromBody] FavoriteApp favorite_app) {
            try {
                conn.Open();

                var query = @"INSERT INTO favorite_apps(user_id, app_id)
                              VALUES (@UserId, @AppId)
                              RETURNING *;";

                var rows = await conn.QueryFirstOrDefaultAsync<object>(query, new {UserId = favorite_app.UserId, AppId = favorite_app.AppId });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully added to the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("delete")]
        [HttpDelete]
        public async Task<IActionResult> deleteFavoriteApp([FromBody] FavoriteApp favorite_app) {
            try {
                conn.Open();
                _logger.LogInformation("Successfully connected to PostgreSQL.");

                var query = "DELETE FROM favorite_apps WHERE user_id = @UserId AND app_id = @AppId";

                var result = await conn.ExecuteAsync(query, new { UserId = favorite_app.UserId, AppId = favorite_app.AppId });

                if (result > 0)
                {
                    return Ok(new { success = true, message = "Data successfully deleted from the database.", data = result });
                }
                else
                {
                    return Ok(new { success = false, message = "Endpoint not found or an error occurred while deleting data.", data = new List<object>() });
                }
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }
    }
    public class FavoriteApp
    {
            public int UserId { get; set; }
            public int AppId { get; set; }
    }
}
