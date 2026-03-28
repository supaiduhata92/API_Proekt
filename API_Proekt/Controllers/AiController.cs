using System;
using System.Threading.Tasks;
using API_Proekt.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_Proekt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly GeminiService _gemini;

        public AiController(GeminiService gemini)
        {
            _gemini = gemini;
        }

        public class BreedRequest
        {
            public string? BreedName { get; set; }
        }

        [HttpPost("breed-info")]
        public async Task<IActionResult> BreedInfo([FromBody] BreedRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.BreedName))
                return BadRequest("breedName is required.");

            var prompt = $"Write a friendly 2–3 paragraph description of the {req.BreedName} breed. Keep it concise and human-friendly.";

            try
            {
                var result = await _gemini.GenerateTextAsync(prompt, maxOutputTokens: 450, temperature: 0.2);
                return Ok(new { source = "gemini", text = result });
            }
            catch (Exception ex)
            {
                return StatusCode(502, $"AI generation failed: {ex.Message}");
            }
        }
    }
}