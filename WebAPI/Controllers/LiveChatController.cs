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
    [Route("/api/db/tables/live-chat")]
    [EnableCors("AllowSpecificOrigin")]

    public class LiveChatController : Controller
    {
        private readonly ILogger<LiveChatController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public LiveChatController(ILogger<LiveChatController> logger)
        {
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                       "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        

        
        [Route("get")]
        [HttpGet]
        public async Task<IActionResult> getMessages([FromQuery] int sender_id) {
            try {
                conn.Open();

                var query = @"SELECT *
                              FROM messages
                              WHERE sender_id = @SenderId OR receiver_id = @SenderId";

                var rows = await conn.QueryAsync<dynamic>(query, new { SenderId = sender_id });

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
        public async Task<IActionResult> addMessages([FromBody] Message message) {
            try {
                conn.Open();

                var query = @"INSERT INTO messages(sender_id, content, receiver_id, attachment_public_id)
                              VALUES (@SenderId, @Content, @ReceiverId, @AttachmentPublicId)
                              RETURNING id";

                var id = await conn.QueryFirstOrDefaultAsync<int>(query, new {
                    SenderId = message.SenderId,
                    Content = message.Content,
                    ReceiverId = message.ReceiverId,
                    AttachmentPublicId = message.AttachmentPublicId
                });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully added to the database.", data = new { id } });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }
    }
    public class Message
    {
            public int SenderId { get; set; }
            public string Content { get; set; }
            public int ReceiverId { get; set; }
            public string AttachmentPublicId { get; set; }
    }
}
