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
using System.Transactions;
using webapi_csharp.Models;

namespace webapi_csharp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationsController : Controller
    {
        private readonly ILogger<NotificationsController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public NotificationsController(ILogger<NotificationsController> logger)
        {
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                       "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        

        [Route("get/user_id")]
        [HttpGet]
        public async Task<IActionResult> getNotificationsByUserId([FromQuery] int user_id) {
            try {
                conn.Open();
                _logger.LogInformation("Successfully connected to PostgreSQL.");

                var query = "SELECT * FROM notifications WHERE user_id = @UserId";

                var rows = await conn.QueryAsync<Header>(query, new { UserId = user_id });

                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> addNotification([FromBody] Notification notification) {
            try {
                conn.Open();

                var rows = await Notification.create(conn, notification.UserId, notification.Title, notification.Description);

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully added to the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("delete")]
        [HttpPost]
        public async Task<IActionResult> deleteNotification([FromBody] Notification notification) {
            try {
                conn.Open();

                var rows = await Notification.delete(conn, notification.Id);

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully added to the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("update-seen/id/seen")]
        [HttpPost]
        public async Task<IActionResult> seenNotification([FromBody] Notification notification) {
            try {
                conn.Open();

                var rows = await Notification.seenById(conn, notification.Id, notification.Seen);

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully added to the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }
    }
}