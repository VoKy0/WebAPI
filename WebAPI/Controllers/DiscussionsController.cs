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
    [Route("/api/db/tables/service-discussions")]
    [EnableCors("AllowSpecificOrigin")]
    public class DiscussionsController : Controller
    {
        private readonly ILogger<DiscussionsController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public DiscussionsController(ILogger<DiscussionsController> logger)
        {
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                              "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        

        

        
        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> addDiscussion([FromBody] Discussion discussion) {
            try {
                conn.Open();

                var query = @"INSERT INTO service_discussions (title, description, user_id, service_id) 
                              VALUES (@Title, @Description, @UserId, @ServiceId); 
                              SELECT SCOPE_IDENTITY();";

                var id = await conn.QueryFirstOrDefaultAsync<int>(query, discussion);

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully added to the database.", data = new { id } });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("get/service_id")]
        [HttpGet]
        public async Task<IActionResult> getDiscussionsByServiceId([FromQuery] int service_id) {
            try {
                conn.Open();

                var query = @"SELECT * 
                              FROM service_discussions 
                              WHERE service_id = @ServiceId 
                              ORDER BY created_at DESC";

                var rows = await conn.QueryAsync<dynamic>(query, new { ServiceId = service_id });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("get/id")]
        [HttpGet]
        public async Task<IActionResult> getDiscussionById([FromQuery] int id) {
            try {
                conn.Open();

                var query = @"SELECT * 
                              FROM service_discussions 
                              WHERE id = @DiscussionId";

                var rows = await conn.QueryAsync<dynamic>(query, new { DiscussionId = id });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }
    }

    public class Discussion
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; }
        public int ServiceId { get; set; }
    }
}