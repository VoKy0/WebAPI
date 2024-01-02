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

using webapi_csharp.services;

namespace webapi_csharp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CloudinaryController : Controller
    {
        private readonly CloudinaryServices _cloudinaryProvider;
        private readonly ILogger<CloudinaryController> _logger;
        private string connString;
        private NpgsqlConnection conn;
        public CloudinaryController(ILogger<CloudinaryController> logger)
        {
            _cloudinaryProvider = new CloudinaryServices();
            _logger = logger;
            connString = string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",
                                              "103.42.57.126", 5432, "admin", "sa123456", "ai-platform");
            conn = new NpgsqlConnection(connString);
        }
        


        [HttpPost("create")]
        public async Task<IActionResult> uploadImage([FromBody] Image request)
        {
            try
            {
                var result = await _cloudinaryProvider.uploadImage(request.ImageData);
                return Ok(new 
                {
                    success = true,
                    message = "Data successfully added to the database.",
                    data = [result]
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new 
                {
                    success = false,
                    message = ex.Message,
                    data = new List<object>()
                });
            }
        }

        [HttpPost("delete")]
        public async Task<IActionResult> deleteImage([FromBody] Image request)
        {
            try
            {
                var isDestroyed = await _cloudinaryProvider.destroyImage(request.PublicId);
                return isDestroyed
                    ? Ok(new { success = true, message = "Image successfully deleted from Cloudinary.", data = new List<object>() })
                    : Ok(new { success = false, message = "Image cannot be deleted from Cloudinary.", data = new List<object>() });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error here: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    data = new List<object>()
                });
            }
        }

        [HttpGet("get")]
        public IActionResult fetchImage([FromQuery] string public_id)
        {
            try
            {
                var imageUrl = _cloudinaryProvider.fetchUrl(public_id);
                return Ok(new
                {
                    success = true,
                    message = "Fetch URL successfully",
                    data = new[] { new { Url = imageUrl } }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error here: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    data = new List<object>()
                });
            }
        }

    public class Image
    {
            public int PublicId {get; set;}
            public string ImageData {get; set;}
    }
}