using Asp.Versioning;
using Azure.Core;
using CatalogService.Transversal.Classes.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class AuthController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("token")]
        public async Task<IActionResult> GetToken([FromBody] LoginRequest loginRequest)
        {
            var client = _httpClientFactory.CreateClient();
            var tokenEndpoint = "https://localhost:5001/connect/token";


            var form = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "client_id", "catalog-api-client" },
                { "client_secret", "catalog-secret" },
                { "username", loginRequest.Username },
                { "password", loginRequest.Password },
                { "scope", "openid profile role manager.read customer.read manager.create manager.update manager.delete offline_access" }
            };



            var response = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(form));
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, content);
            }

            return Content(content, "application/json");
        }
    }
}
