using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PomodoroAppBackend.Models;
using PomodoroAppBackend.Context;
using Microsoft.EntityFrameworkCore;
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
        private readonly ApplicationDBContext _context;

        public AIController(HttpClient httpClient, IConfiguration configuration, ApplicationDBContext context)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
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

        [HttpPost("save-note")]
        public async Task<IActionResult> SaveNote([FromBody] Note newNote)
        {
            if (string.IsNullOrWhiteSpace(newNote.Topic) || newNote.Cues == null || newNote.SuccinctNotes == null)
            {
                return BadRequest("Please provide valid data for the note.");
            }

            // Fetch Subject
            var subject = await _context.Subjects.FindAsync(newNote.SubjectId);
            if (subject == null)
            {
                return BadRequest("Subject not found.");
            }

            var note = new Note
            {
                Topic = newNote.Topic,
                Summary = newNote.Summary,
                Subject = subject,
                Cues = newNote.Cues.Select(cue => new Cue { Text = cue.Text }).ToList(),
                SuccinctNotes = newNote.SuccinctNotes.Select(succNote => new SuccinctNote { Summary = succNote.Summary }).ToList()
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Note saved successfully." });
        }

        [HttpGet("get-notes")]
        public async Task<IActionResult> GetNotes()
        {
            var notes = await _context.Notes
                .Include(n => n.Subject) // Include the related Subject
                .Include(n => n.Cues) // Include the related Cues
                .Include(n => n.SuccinctNotes) // Include the related Succinct Notes
                .ToListAsync();

            return Ok(notes);
        }

        [HttpPost("generate-note")]
        public async Task<IActionResult> GenerateNote([FromBody] string inputText)
        {
            if (string.IsNullOrWhiteSpace(inputText))
            {
                return BadRequest("Input text cannot be empty.");
            }

            var apiKey = _configuration["OpenAI:ApiKey"];
            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[]
                {
                    new { role = "user", prompt = $"Generate a Cornell note from the following text: {inputText}" }
                },
                max_tokens = 250
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            requestMessage.Headers.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Error response content: " + errorContent);  // Log error content to the console for debugging
                return StatusCode((int)response.StatusCode, "Failed to generate note.");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var generatedNote = jsonResponse.GetProperty("choices")[0].GetProperty("text").GetString();

            var newNote = new Note
            {
                Topic = "Generated Topic",
                // Cues should be a List<string>, so we wrap the cue in a list
                Cues = new List<Cue> { new Cue { Text = "Generated Cue" } },  // Create Cue objects, not strings
                SuccinctNotes = new List<SuccinctNote> { new SuccinctNote { Summary = generatedNote } },
                Summary = "Generated Summary"
            };

            // Save the generated note into the database (optional)
            _context.Notes.Add(newNote);
            await _context.SaveChangesAsync();

            return Ok(newNote);
        }

       [HttpPost("generate-quiz")]
        public async Task<IActionResult> GenerateQuiz([FromBody] string noteText)
        {
            if (string.IsNullOrWhiteSpace(noteText))
            {
                return BadRequest("Note text cannot be empty.");
            }

            var apiKey = _configuration["OpenAI:ApiKey"];
            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[] 
                {
                    new { role = "user", content = $"Generate a quiz based on the following note: {noteText}" }
                },
                max_tokens = 100
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            requestMessage.Headers.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to generate quiz.");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var quiz = jsonResponse.GetProperty("choices")[0].GetProperty("text").GetString();

            return Ok(new { quiz = quiz });
        }


    }
}
