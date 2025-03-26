using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PomodoroAppBackend.Models;
using PomodoroAppBackend.Context;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using PomodoroAppBackend.DTOs;
using Azure.Messaging;
using System.Text.Json.Serialization;
using PomodoroAppBackend.Models.RequestObjects;
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

        [HttpPost("save-note2")] //This is with subject generation 
        public async Task<IActionResult> SaveNote2([FromBody] NoteDto newNote)
        {
            Console.WriteLine("Received Note: " + JsonSerializer.Serialize(newNote));
            if (string.IsNullOrWhiteSpace(newNote.Topic) || newNote.Cues == null || newNote.SuccinctNotes == null)
            {
                var errorMessage = new StringBuilder("Please provide valid data for the note. ");

                if (string.IsNullOrWhiteSpace(newNote.Topic))
                {
                    errorMessage.Append($"Topic is invalid: '{newNote.Topic}'. ");
                }

                if (newNote.Cues == null)
                {
                    errorMessage.Append("Cues are null. ");
                }

                if (newNote.SuccinctNotes == null)
                {
                    errorMessage.Append("SuccinctNotes are null. ");
                }

                return BadRequest(errorMessage.ToString());
            }

            // Check if a valid SubjectId is provided
            var subject = await _context.Subjects.FindAsync(newNote.SubjectId);

            if (subject == null)
            {
                // If subject is not found, check if SubjectName is provided
                if (string.IsNullOrWhiteSpace(newNote.SubjectName))
                {
                    return BadRequest("Subject not found and no subject name provided.");
                }

                // Check if a subject with the same name already exists
                subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Name == newNote.SubjectName);
                if (subject == null)
                {
                    // Create a new Subject if not found
                    subject = new Subject { Name = newNote.SubjectName };
                    _context.Subjects.Add(subject);
                    await _context.SaveChangesAsync();  // Save the new subject to get its ID
                }
            }

            // Create the new Note
            var note = new Note
            {
                Topic = newNote.Topic,
                Summary = newNote.Summary,
                SubjectId = subject.SubjectId,  // Associate the subject
                Subject = subject,  // Optionally link the full subject object
                Cues = newNote.Cues.Select(cue => new Cue { Text = cue }).ToList(),
                SuccinctNotes = newNote.SuccinctNotes.Select(succNote => new SuccinctNote { Summary = succNote }).ToList()
            };

            // Add the new Note to the context and save it
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            // Return the created note or a message confirming successful creation
            return CreatedAtAction(nameof(GetNoteById), new { id = note.NoteId }, note); // Assuming a GetNoteById action exists
        }

        [HttpPut("update-note/{id}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] NoteDto updatedNote)
        {

            // Log the incoming request body
            Console.WriteLine("Received UpdateNote request: " + JsonSerializer.Serialize(updatedNote));

            var note = await _context.Notes
                .Include(n => n.Cues)
                .Include(n => n.SuccinctNotes)
                .FirstOrDefaultAsync(n => n.NoteId == id);

            if (note == null)
            {
                return NotFound("Note not found.");
            }

            if (string.IsNullOrWhiteSpace(updatedNote.Topic) || updatedNote.Cues == null || updatedNote.SuccinctNotes == null)
            {
                return BadRequest("Please provide valid data for the note.");
            }

            // Update the note properties
            note.Topic = updatedNote.Topic;
            note.Summary = updatedNote.Summary;

            // Update the Cues
            note.Cues.Clear();
            note.Cues.AddRange(updatedNote.Cues.Select(cue => new Cue { Text = cue }));

            // Update the SuccinctNotes
            note.SuccinctNotes.Clear();
            note.SuccinctNotes.AddRange(updatedNote.SuccinctNotes.Select(succNote => new SuccinctNote { Summary = succNote }));

            // Save changes to the database
            await _context.SaveChangesAsync(); //TODO: Edit is buggy here maybe

            return Ok(note);
        }

        [HttpDelete("delete-note/{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var note = await _context.Notes.FindAsync(id);

            if (note == null)
            {
                return NotFound("Note not found.");
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Note deleted successfully." });
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

        [HttpGet("get-note/{id}")]
        public async Task<IActionResult> GetNoteById(int id)
        {
            var note = await _context.Notes
                .Include(n => n.Cues)
                .Include(n => n.SuccinctNotes)
                .Include(n => n.Subject)  // Ensure Subject is included in the result
                .FirstOrDefaultAsync(n => n.NoteId == id);

            if (note == null)
            {
                return NotFound();
            }

            return Ok(note);
        }

        //[HttpGet("get-all-subjects")]
        //public async Task<IActionResult> GetAllSubjects()
        //{
        //    // Query the database for all subjects
        //    var subjects = await _context.Subjects
        //        .Include(s => s.Notes) // Optionally include related Notes
        //        .ToListAsync();

        //    // Check if any subjects were found
        //    if (subjects == null || subjects.Count == 0)
        //    {
        //        return NotFound("No subjects found.");
        //    }

        //    // Return the list of subjects
        //    return Ok(subjects);
        //}

        [HttpGet("get-notes-by-subject/{subjectId}")]
        public async Task<IActionResult> GetNotesBySubjectId(int subjectId)
        {
            // Query the database for notes with the specified subject ID
            var notes = await _context.Notes
                .Include(n => n.Subject) // Include the related Subject
                .Include(n => n.Cues) // Include the related Cues
                .Include(n => n.SuccinctNotes) // Include the related Succinct Notes
                .Where(n => n.SubjectId == subjectId)
                .ToListAsync();

            // Check if any notes were found
            if (notes == null || notes.Count == 0)
            {
                return NotFound("No notes found for the specified subject ID.");
            }

            // Return the list of notes
            return Ok(notes);
        }

        [HttpDelete("delete-subject/{subjectId}")]
        public async Task<IActionResult> DeleteSubject(int subjectId)
        {
            // Retrieve the subject
            var subject = await _context.Subjects.FindAsync(subjectId);

            if (subject == null)
            {
                return NotFound("Subject not found.");
            }

            // Retrieve the notes associated with the subject
            var notes = await _context.Notes
                .Where(n => n.SubjectId == subjectId)
                .ToListAsync();

            // Remove the notes
            _context.Notes.RemoveRange(notes);

            // Remove the subject
            _context.Subjects.Remove(subject);

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(new { message = "Subject and its notes deleted successfully." });
        }


        [HttpPost("generate-note")]
        public async Task<IActionResult> GenerateNote([FromBody] GenerateNoteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.InputText))
            {
                return BadRequest("Input text cannot be empty.");
            }

            // Escape newlines and other special characters
            var escapedNoteText = request.InputText.Replace("\n", "\\n").Replace("\r", "\\r");

            var apiKey = _configuration["OpenAI:ApiKey"];
            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[]
                {
            new { role = "system", content = "You are a helpful assistant for learning. When asked to generate a Cornell note, its crucial to return a note in a JSON format, dont write anything outside of the Json format, DO NOT WRAP THE JSON CODES IN JSON MARKERS, so I can extract data to objects easily, follow this {\r\n  \"topic\": \"{Topic}\",\r\n  \"summary\": \"{Summary}\",\r\n  \"cues\": [\"{Cue1}\", \"{Cue2}\", \"{Cue3}\", ...],\r\n  \"succinctNotes\": [\"{SuccinctNote1}\", \"{SuccinctNote2}\", \"{SuccinctNote3}\", ...]\r\n}" },
            new { role = "user", content = $"Generate a Cornell note from the following text: {escapedNoteText}. Use JSON for the response. The summary should keep most of the entry text, don't shorten it much." }
        },
                max_tokens = 600
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
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
            Console.WriteLine("Full response content: " + responseContent);

            // Extract the message content from the response
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            Console.WriteLine("Json.Deserialize : " + jsonResponse);
            var messageContent = jsonResponse.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            Console.WriteLine("messageContent : " + messageContent);

            if (string.IsNullOrWhiteSpace(messageContent))
            {
                return BadRequest("Failed to parse the generated note.");
            }

            // Deserialize the message content into a Note object
            NoteDto generatedNoteDto;
            try
            {
                generatedNoteDto = JsonSerializer.Deserialize<NoteDto>(messageContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                Console.WriteLine("Deserialized Note object: " + JsonSerializer.Serialize(generatedNoteDto));
            }
            catch (JsonException ex)
            {
                Console.WriteLine("Failed to deserialize AI response: " + ex.Message);
                return BadRequest("Failed to parse the generated note.");
            }

            if (generatedNoteDto == null)
            {
                return BadRequest("Failed to parse the generated note.");
            }

            // Fetch or create the subject dynamically
            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Name == "Math");
            if (subject == null)
            {
                subject = new Subject { Name = "Math" };
                _context.Subjects.Add(subject);
                await _context.SaveChangesAsync();
            }

            // Create the new note with multiple cues and succinct notes
            var newNote = new Note
            {
                Topic = generatedNoteDto.Topic ?? "Default Topic",
                Summary = generatedNoteDto.Summary ?? "Default Summary",
                Cues = generatedNoteDto.Cues?.Select(cue => new Cue { Text = cue }).ToList() ?? new List<Cue> { new Cue { Text = "Default Cue" } },
                SuccinctNotes = generatedNoteDto.SuccinctNotes?.Select(succinctNote => new SuccinctNote { Summary = succinctNote }).ToList() ?? new List<SuccinctNote> { new SuccinctNote { Summary = "Default Succinct Note" } },
                Subject = subject
            };

            // Save the generated note into the database
            _context.Notes.Add(newNote);
            await _context.SaveChangesAsync();

            return Ok(newNote);
        }


        [HttpPost("test-generate-quiz")]
        public async Task<IActionResult> GenerateQuiz1([FromBody] GenerateQuizRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NoteText))
            {
                return BadRequest("Note text cannot be empty.");
            }
            Console.WriteLine("Note text: " + request.NoteText);
            // Replace newline, carriage return, and tab characters with a space
            var escapedNoteText = request.NoteText.Replace("\n", " ")  // Replace newlines with a space
                .Replace("\r", " ")  // Replace carriage returns with a space
                .Replace("\t", " ");  // Replace tabs with a space (optional)

            var apiKey = _configuration["OpenAI:ApiKey"];
            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[] 
                {
                    new { role = "user", content = $"Generate a quiz based on the following note: {escapedNoteText}" }
                },
                max_tokens = 250
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            requestMessage.Headers.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Error response content: " + errorContent);  // Log error content to the console for debugging
                return StatusCode((int)response.StatusCode, "Failed to generate quiz.");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Full response content: " + responseContent);
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

            var quiz = jsonResponse.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            return Ok(new { response = quiz });
        }
        [HttpPost("generate-quiz")]
        public async Task<IActionResult> GenerateQuiz2([FromBody] GenerateQuizRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NoteText))
            {
                return BadRequest("Note text cannot be empty.");
            }

            Console.WriteLine("Note text: " + request.NoteText);

            // Escape newlines and other special characters
            var escapedNoteText = request.NoteText.Replace("\n", " ").Replace("\r", " ").Replace("\t", " ");

            var apiKey = _configuration["OpenAI:ApiKey"];
            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[]
                {
                    new {
                        role = "system",
                        content = "You are a helpful assistant for learning. When asked to generate a quiz, it’s crucial to return the quiz in a JSON format. Don't write anything outside of the JSON format, DO NOT WRAP THE JSON CODES IN JSON MARKERS, so I can extract data to objects easily. Follow this format: {\"questions\":[{\"question\":\"{QuestionText}\",\"options\":[\"{Option1}\",\"{Option2}\",\"{Option3}\",\"{Option4}\"],\"correctAnswer\":\"{CorrectAnswer}\"},{\"question\":\"{QuestionText}\",\"options\":[\"{Option1}\",\"{Option2}\",\"{Option3}\",\"{Option4}\"],\"correctAnswer\":\"{CorrectAnswer}\"}]}\"}"
                    },
                    new { role = "user", content = $"Generate a quiz based on the following note: {escapedNoteText} Use JSON format" }
                },
                max_tokens = 500
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            requestMessage.Headers.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Error response content: " + errorContent);
                return StatusCode((int)response.StatusCode, "Failed to generate quiz.");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Full response content: " + responseContent);
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            Console.WriteLine("Json.Deserialize " + jsonResponse);

            var quizContent = jsonResponse.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            Console.WriteLine("messageContent :  " + quizContent);

            if (string.IsNullOrWhiteSpace(quizContent))
            {
                return BadRequest("Failed to parse the generated quiz.");
            }

            // Deserialize the quiz content into a QuizDto object
            QuizDto? generatedQuizDto;
            try
            {
                generatedQuizDto = JsonSerializer.Deserialize<QuizDto>(quizContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });
                Console.WriteLine("Deserialized Quiz object: " + JsonSerializer.Serialize(generatedQuizDto));
            }
            catch (JsonException ex)
            {
                Console.WriteLine("Failed to deserialize AI response: " + ex.Message);
                return BadRequest("Failed to parse the generated quiz.");
            }

            if (generatedQuizDto == null)
            {
                return BadRequest("Failed to parse the generated quiz.");
            }

            // Fetch the note dynamically
            var note = await _context.Notes.Include(n => n.Quizzes).FirstOrDefaultAsync(n => n.NoteId == request.NoteId);
            if (note == null)
            {
                return BadRequest("Note not found.");
            }

            // Create the new quiz
            var newQuiz = new Quiz
            {
                Questions = generatedQuizDto.Questions.Select(q => new Question
                {
                    QuestionText = q.Question,
                    CorrectAnswer = q.CorrectAnswer,
                    Options = q.Options
                }).ToList()
            };

            // Add the new quiz to the note
            note.Quizzes.Add(newQuiz);

            // Save the changes into the database
            await _context.SaveChangesAsync();

            return Ok(newQuiz);
        }

    }
}
