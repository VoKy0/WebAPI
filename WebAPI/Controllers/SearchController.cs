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
    public class SearchController : ControllerBase
    {
        private readonly ILogger<SearchController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public SearchController(ILogger<SearchController> logger)
        {
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                              "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        

        

        [HttpGet]
        public async Task<IActionResult> searchServices([FromQuery] string input) {
            try {
                conn.Open();

                var query = "SELECT * FROM services WHERE LOWER(name) LIKE @Input";
                var rows = await conn.QueryAsync<Service>(query, new { Input = $"%{input.ToLower()}%" });                

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> searchApps([FromQuery] string input) {
            try {
                conn.Open();

                var query = @"SELECT apps.*, services.vendor_id, services.updated_at, services.service_category_id 
                              FROM apps, services
                              WHERE apps.service_id = services.id AND LOWER(apps.name) LIKE @Input";
                var rows = await conn.QueryAsync<dynamic>(query, new { Input = $"%{input.ToLower()}%" });                

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