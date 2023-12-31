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
    [Route("/api/db/tables/apps")]
    [EnableCors("AllowSpecificOrigin")]
    public class AppsController : Controller
    {
        private readonly ILogger<AppsController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public AppsController(ILogger<AppsController> logger)
        {
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                              "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        

        
        [Route("get")]
        [HttpGet]   
        public async Task<IActionResult> getApps() {
            try {
                conn.Open();

                var query = @"SELECT apps.*, services.vendor_id
                              FROM apps, services
                              WHERE apps.service_id = services.id";
                var rows = await conn.QueryAsync<dynamic>(query);                

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
        public async Task<IActionResult> addApp([FromBody] App app) {
            try {
                conn.Open();

                var query = @"INSERT INTO apps(name, description, vendor_id, service_category_id)
                              VALUES (@Name, @Description, @VendorId, @ServiceCategoryId);
                              SELECT SCOPE_IDENTITY();";

                var id = await conn.QueryFirstOrDefaultAsync<int>(query, app);

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully added to the database.", data = new { id } });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("get/service_category_name")]
        [HttpGet]
        public async Task<IActionResult> getAppsByServiceCategoryName([FromQuery] string service_category_name) {
            try {
                conn.Open();

                var query = @"SELECT apps.*
                              FROM apps, service_categories
                              WHERE apps.service_id = service_categories.id AND service_categories.name = @ServiceCategoryName";

                var rows = await conn.QueryAsync<dynamic>(query, new { ServiceCategoryName = service_category_name });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("get/service_category_id")]
        [HttpGet]
        public async Task<IActionResult> getAppsByServiceCategoryId([FromQuery] string service_category_id) {
            try {
                conn.Open();

                var query = @"SELECT *
                              FROM apps
                              WHERE service_category_id = @ServiceCategoryId";

                var rows = await conn.QueryAsync<dynamic>(query, new { ServiceCategoryId = service_category_id });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("get/vendor_id")]
        [HttpGet]
        public async Task<IActionResult> getAppsByVendorId([FromQuery] int vendor_id) {
            try {
                conn.Open();

                var query = @"SELECT apps.*
                              FROM apps, services
                              WHERE apps.service_id = services.id AND services.vendor_id = @VendorId";

                var rows = await conn.QueryAsync<dynamic>(query, new { VendorId = vendor_id });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }
    }

    public class App
    {
            public string Name { get; set; }
            public string Description { get; set; }
            public int VendorId { get; set; }
            public int ServiceCategoryId { get; set; }
    }
}