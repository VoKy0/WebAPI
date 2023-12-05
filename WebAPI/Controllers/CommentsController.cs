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
    public class CommentsController : ControllerBase
    {
        private readonly ILogger<CommentsController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public CommentsController(ILogger<CommentsController> logger)
        {
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                              "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        

        
        
        [HttpPost]
        public async Task<IActionResult> addComment([FromBody] Comment comment) {
            try {
                conn.Open();

                var query = @"INSERT INTO discussion_comments (comment, user_id, service_discussion_id)
                              VALUES (@Comment, @UserId, @ServiceDiscussionId);
                              SELECT SCOPE_IDENTITY();";

                var id = await conn.QueryFirstOrDefaultAsync<int>(query, comment);

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully added to the database.", data = new { id } });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [HttpGet]
        public async Task<IActionResult> getCommentsByDiscussionId([FromQuery] int discussion_id) {
            try {
                conn.Open();

                var query = @"SELECT *
                              FROM discussion_comments
                              WHERE service_discussion_id = @DiscussionId";

                var rows = await conn.QueryAsync<dynamic>(query, new { DiscussionId = discussion_id });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }
    }

    public class Comment
    {
        public string Content { get; set; }
        public int UserId { get; set; }
        public int ServiceDiscussionId { get; set; }
    }
}