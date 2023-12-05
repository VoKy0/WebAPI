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
    public class ServiceCategoriesController : Controller
    {
        private readonly ILogger<ServiceCategoriesController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public ServiceCategoriesController(ILogger<ServiceCategoriesController> logger)
        {
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                              "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        

        
        [Route("get")]
        [HttpGet]
        public async Task<IActionResult> getServiceCategories() {
            try {
                conn.Open();

                var query = "SELECT * FROM service_categories";
                var rows = await conn.QueryAsync<ServiceCategory>(query);                

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
        public async Task<IActionResult> getServiceCategoryById(int id) {
            try {
                conn.Open();

                var query = "SELECT * FROM service_categories WHERE id = @Id";
                var rows = await conn.QueryAsync<ServiceCategory>(query, new { Id = id });                

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }
    }

    public class ServiceCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}