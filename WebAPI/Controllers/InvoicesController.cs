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
    public class InvoicesController : Controller
    {
        private readonly ILogger<InvoicesController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public InvoicesController(ILogger<InvoicesController> logger)
        {
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                       "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        

        
        [Route("get/id")]
        [HttpGet]
        public async Task<IActionResult> getInvoiceById([FromQuery] int id) {
            try {
                conn.Open();

                var query = @"SELECT *
                              FROM invoices
                              WHERE id = @Id";

                var rows = await conn.QueryAsync<int>(query, new { Id = id });

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
        public async Task<IActionResult> addInvoice([FromBody] Invoice invoice) {
            try {
                conn.Open();

                var query = @"INSERT INTO invoices(date, currency_code, tax_rate, tax_amount, gross_amount, net_amount)
                              VALUES (@Date, @CurrencyCode, @TaxRate, @TaxAmount, @GrossAmount, @NetAmount)
                              RETURNING *;";

                var rows = await conn.QueryFirstOrDefaultAsync<object>(query, new {Date = invoice.Date, 
                                                                                   CurrencyCode = invoice.CurrencyCode, 
                                                                                   TaxRate = invoice.TaxRate, 
                                                                                   TaxAmount = invoice.TaxAmount, 
                                                                                   GrossAmount = invoice.GrossAmount,
                                                                                   NetAmount = invoice.NetAmount});

                _logger.LogInformation("Successfully connected to PostgreSQL.");
                return Ok(new { success = true, message = "Data successfully added to the database.", data = rows });
            }
            catch (Exception ex) {
                _logger.LogError("Failed to connect to PostgreSQL. Error: " + ex.Message);
                return StatusCode(500, new { success = false, message = ex.Message, data = new List<object>() });
            }
        }
    }
    public class Invoice
    {
            public DateTime Date { get; set; }
            public string CurrencyCode { get; set; }
            public decimal TaxRate {get; set;}
            public decimal TaxAmount {get; set;}
            public decimal GrossAmount {get; set;}
            public decimal NetAmount {get; set;}
    }
}
