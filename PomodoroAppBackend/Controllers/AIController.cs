using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace PomodoroAppBackend.Controllers
{
    [Route("api/ai")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public AIController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        [HttpPost("hold-thought")]
        public IActionResult HoldThought([FromBody] string thought)
        {
            if (string.IsNullOrWhiteSpace(thought))
            {
                return BadRequest("Thought cannot be empty.");
            }

            // Retrieve the current list of thoughts from the session
            var existingThoughtsJson = HttpContext.Session.GetString("thoughts");
            List<string> thoughts = string.IsNullOrEmpty(existingThoughtsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(existingThoughtsJson) ?? new List<string>();

            // Add the new thought to the list
            thoughts.Add(thought);

            // Save the updated list back to the session
            HttpContext.Session.SetString("thoughts", JsonSerializer.Serialize(thoughts));

            return Ok(new { message = "Thought recorded." });
        }

        //just a debug test are there thoughts stored in the session
        [HttpGet("check-thoughts")]
        public IActionResult CheckThoughts()
        {
            // Retrieve the list of thoughts from the session
            var existingThoughtsJson = HttpContext.Session.GetString("thoughts");
            if (string.IsNullOrEmpty(existingThoughtsJson))
            {
                return Ok("No thoughts found in session.");
            }

            // Deserialize thoughts from session
            List<string> thoughts = JsonSerializer.Deserialize<List<string>>(existingThoughtsJson) ?? new List<string>();

            // Return the list of thoughts
            return Ok(thoughts);
        }


        [HttpGet("respond")]
        public async Task<IActionResult> GetAIResponse()
        {
            // Retrieve the list of thoughts
            var existingThoughtsJson = HttpContext.Session.GetString("thoughts");
            if (string.IsNullOrEmpty(existingThoughtsJson))
            {
                return BadRequest("No thoughts found for this session.");
            }

            List<string> thoughts = string.IsNullOrEmpty(existingThoughtsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(existingThoughtsJson) ?? new List<string>();


            // For simplicity, concatenate thoughts into a single prompt
            var combinedThoughts = string.Join("\n", thoughts);

            // Clear the thoughts after generating the response
            HttpContext.Session.Remove("thoughts");

            var apiKey = _configuration["OpenAI:ApiKey"]; 
            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[]
                    {
                        new { role = "user", content = $"Respond to these thoughts:\n{combinedThoughts}" }
                    },
                max_tokens = 100
            };

            // Create the HttpRequestMessage to include the Authorization header
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            requestMessage.Headers.Add("Authorization", $"Bearer {apiKey}");

            // Send the request
            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                // Log the error response content for debugging
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {errorContent}");
                return StatusCode((int)response.StatusCode, "Failed to get AI response.");
            }


            // Parse and return the response
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("responseContent");
            Console.WriteLine(responseContent);
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            return Ok(new { response = jsonResponse.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() });
        }
    }
}
