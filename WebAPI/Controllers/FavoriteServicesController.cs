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

namespace webapi_csharp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FavoriteServicesController : Controller
    {
        private readonly ILogger<FavoriteServicesController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public FavoriteServicesController(ILogger<FavoriteServicesController> logger)
        {
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                       "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        

        
        [Route("get-count/service_id")]
        [HttpGet]
        public async Task<IActionResult> getFavoriteCountByServiceId([FromQuery] int service_id) {
            try {
                conn.Open();

                var query = @"SELECT COUNT(*) AS favorite_count
                              FROM favorite_services
                              WHERE service_id = @ServiceId
                              GROUP BY service_id";

                var rows = await conn.QueryAsync<int>(query, new { ServiceId = service_id });

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
        public async Task<IActionResult> getFavoriteServiceByDetails([FromQuery] int user_id, [FromQuery] int service_id) {
            try {
                conn.Open();

                var query = @"SELECT *
                              FROM favorite_services
                              WHERE user_id = @UserId AND service_id = @ServiceId";

                var rows = await conn.QueryAsync<object>(query, new { UserId = user_id, ServiceId = service_id });

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
        public async Task<IActionResult> addFavoriteService([FromBody] FavoriteService favorite_service) {
            try {
                conn.Open();

                var query = @"INSERT INTO favorite_services(user_id, service_id)
                              VALUES (@UserId, @ServiceId)
                              RETURNING *;";

                var rows = await conn.QueryFirstOrDefaultAsync<object>(query, new {UserId = favorite_service.UserId, ServiceId = favorite_service.ServiceId });

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
        public async Task<IActionResult> deleteFavoriteService([FromBody] FavoriteService favorite_service) {
            try {
                conn.Open();
                _logger.LogInformation("Successfully connected to PostgreSQL.");

                var query = "DELETE FROM favorite_services WHERE user_id = @UserId AND service_id = @ServiceId";

                var result = await conn.ExecuteAsync(query, new { UserId = favorite_service.UserId, ServiceId = favorite_service.ServiceId });

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
    public class FavoriteService
    {
            public int UserId { get; set; }
            public int ServiceId { get; set; }
    }
}
