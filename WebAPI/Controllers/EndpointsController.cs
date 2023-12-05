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
    public class EndpointsController : Controller
    {
        private readonly ILogger<EndpointsController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public EndpointsController(ILogger<EndpointsController> logger)
        {
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                              "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        

        

        
        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> addEndpoint([FromBody] EndpointRequest request) {
            try {
                conn.Open();
                _logger.LogInformation("Successfully connected to PostgreSQL.");

                List<EndpointModel> newEndpoint = new List<EndpointModel>();

                using (var transaction = conn.BeginTransaction())
                {
                    var endpoint = request.Endpoint;
                    var queries = request.Queries;
                    var headers = request.Headers;
                    var body = request.Body;

                    var result = await conn.QuerySingleAsync<EndpointModel>(@"
                        INSERT INTO endpoints (endpoint) OUTPUT INSERTED.* VALUES (@Endpoint);
                    ", 
                    new 
                    { 
                        Endpoint = endpoint,
                    },
                    transaction);

                    newEndpoint.Add(result);

                    headers.ForEach(header => header.EndpointId = result.Id);
                    var addHeaders = conn.Execute("INSERT INTO headers (endpoint_id, name, value) VALUES (@EndpointId, @Name, @Value);", headers);

                    queries.ForEach(query => query.EndpointId = result.Id);
                    var addQueries = conn.Execute("INSERT INTO queries (endpoint_id, name, value) VALUES (@EndpointId, @Name, @Value);", queries);

                    body.EndpointId = result.Id;
                    var addBody = conn.Execute("INSERT INTO bodies (endpoint_id, name, value) VALUES (@EndpointId, @Name, @Value);", body);

                    if (addHeaders + addQueries + addBody != 3) {
                        transaction.Rollback();
                    }

                    transaction.Commit();
                };

                if (newEndpoint.Count > 0) {
                    return Ok(new { success = true, message = "Data successfully added to the database.", data = newEndpoint });
                }
                else {
                    return Ok(new { success = false, message = "An error occurred while adding data.", data = newEndpoint });
                }
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("update")]
        [HttpPost]
        public async Task<IActionResult> updateEndpoint([FromBody] EndpointRequest request) {
            try {
                conn.Open();
                _logger.LogInformation("Successfully connected to PostgreSQL.");

                EndpointModel endpoint = null;

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    endpoint = request.Endpoint;
                    var queries = request.Queries;
                    var headers = request.Headers;
                    var body = request.Body;

                    var delQueriesPromise = conn.Execute("DELETE FROM queries WHERE endpoint_id = @EndpointId;", new { EndpointId = endpoint.Id });
                    var delHeadersPromise = conn.Execute("DELETE FROM headers WHERE endpoint_id = @EndpointId;", new { EndpointId = endpoint.Id });
                    var delBody = conn.Execute("DELETE FROM bodies WHERE endpoint_id = @EndpointId;", new { EndpointId = endpoint.Id });
                    
                    var updateEndpointPromise = conn.Execute("UPDATE endpoints SET endpoint = @Endpoint WHERE id = @Id;", new { Endpoint = endpoint.Endpoint, Id = endpoint.Id });

                    headers.ForEach(header => header.EndpointId = endpoint.Id);
                    var addHeaders = conn.Execute("INSERT INTO headers (endpoint_id, name, value) VALUES (@EndpointId, @Name, @Value);", headers);

                    queries.ForEach(query => query.EndpointId = endpoint.Id);
                    var addQueries = conn.Execute("INSERT INTO queries (endpoint_id, name, value) VALUES (@EndpointId, @Name, @Value);", queries);

                    body.EndpointId = endpoint.Id;
                    var addBody = conn.Execute("INSERT INTO bodies (endpoint_id, name, value) VALUES (@EndpointId, @Name, @Value);", body);
                };

                if (endpoint != null)
                {
                    return Ok(new { success = true, message = "Data successfully updated to the database.", data = new List<EndpointModel> { endpoint } });
                }
                else
                {
                    return Ok(new { success = false, message = "An error occurred while updating data.", data = new List<EndpointModel>() });
                }
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("headers/get/endpoint_id")]
        [HttpGet]
        public async Task<IActionResult> getHeadersByEndpointId([FromQuery] int endpoint_id) {
            try {
                conn.Open();
                _logger.LogInformation("Successfully connected to PostgreSQL.");

                var query = "SELECT * FROM headers WHERE endpoint_id = @EndpointId";

                var rows = await conn.QueryAsync<Header>(query, new { EndpointId = endpoint_id });

                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("bodies/get/endpoint_id")]
        [HttpGet]
        public async Task<IActionResult> getBodiesByEndpointId([FromQuery] int endpoint_id) {
            try {
                conn.Open();

                var query = "SELECT * FROM bodies WHERE endpoint_id = @EndpointId";

                var rows = await conn.QueryAsync<Body>(query, new { EndpointId = endpoint_id });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("queries/get/endpoint_id")]
        [HttpGet]
        public async Task<IActionResult> getQueriesByEndpointId([FromQuery] int endpoint_id) {
            try {
                conn.Open();

                var query = "SELECT * FROM queries WHERE endpoint_id = @EndpointId";

                var rows = await conn.QueryAsync<Query>(query, new { EndpointId = endpoint_id });

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        [Route("delete")]
        [HttpDelete]
        public async Task<IActionResult> deleteEndpoint([FromBody] DeleteEndpointRequest request) {
            try {
                conn.Open();
                _logger.LogInformation("Successfully connected to PostgreSQL.");

                var query = "DELETE FROM endpoints WHERE id = @Id";

                var result = await conn.ExecuteAsync(query, request);

                if (result > 0)
                {
                    return Ok(new { success = true, message = "Data successfully deleted from the database.", data = new { id = request.Id } });
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

        [Route("get/service_id")]
        [HttpGet]
        public async Task<IActionResult> getEndpointsByServiceId([FromQuery] int service_id) {
            try {
                conn.Open();
                _logger.LogInformation("Successfully connected to PostgreSQL.");

                var query = "SELECT * FROM endpoints WHERE service_id = @ServiceId";

                var rows = await conn.QueryAsync<EndpointModel>(query, new { ServiceId = service_id });

                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }

        
        [Route("get/service/service_category_id")]
        [HttpGet]
        public async Task<IActionResult> getServicesByServiceCategoryId([FromQuery] int service_category_id) {
            try {
                conn.Open();
                _logger.LogInformation("Successfully connected to PostgreSQL.");

                var query = "SELECT * FROM services WHERE service_category_id = @ServiceCategoryId";

                var rows = await conn.QueryAsync<Service>(query, new { ServiceCategoryId = service_category_id });

                return Ok(new { success = true, message = "Data successfully queried from the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }
    }
    public class EndpointRequest
    {
        public EndpointModel Endpoint { get; set; }
        public List<Query> Queries { get; set; }
        public List<Header> Headers { get; set; }
        public Body Body { get; set; }
    }

    public class DeleteEndpointRequest
    {
        public int Id { get; set; }
    }

    public class EndpointModel
    {
        public int Id { get; set; }
        public string Endpoint { get; set; }
        public int ServiceId { get; set; }
    }

    public class Query
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int EndpointId { get; set; }
    }

    public class Header
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int EndpointId { get; set; }
    }

    public class Body
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int EndpointId { get; set; }
    }
}