
using System.Text.Json;
using API_Proekt.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace API_Proekt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatsController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string SearchUrl = "https://api.thecatapi.com/v1/images/search?limit=1&has_breeds=1";
        private const string ImageByIdUrlTemplate = "https://api.thecatapi.com/v1/images/{0}";

        public CatsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("random")]
        public async Task<IActionResult> GetRandomCat()
        {
            var client = _httpClientFactory.CreateClient();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // 1) search for an image that has breed info
            var searchResp = await client.GetAsync(SearchUrl);
            if (!searchResp.IsSuccessStatusCode)
                return StatusCode((int)searchResp.StatusCode, "Failed to fetch cat search data");

            var searchJson = await searchResp.Content.ReadAsStringAsync();
            List<SearchResult>? searchResults;
            try
            {
                searchResults = JsonSerializer.Deserialize<List<SearchResult>>(searchJson, options);
            }
            catch (JsonException)
            {
                return StatusCode(500, "Failed to parse search response");
            }

            if (searchResults == null || searchResults.Count == 0)
                return NotFound("No cat image returned from search");

            var first = searchResults[0];
            if (string.IsNullOrEmpty(first?.Id))
                return NotFound("Search result did not include an id");

            // 2) request the image details by id (this includes breed info)
            var detailUrl = string.Format(ImageByIdUrlTemplate, first.Id);
            var detailResp = await client.GetAsync(detailUrl);
            if (!detailResp.IsSuccessStatusCode)
                return StatusCode((int)detailResp.StatusCode, "Failed to fetch cat details");

            var detailJson = await detailResp.Content.ReadAsStringAsync();
            ImageDetail? detail;
            try
            {
                detail = JsonSerializer.Deserialize<ImageDetail>(detailJson, options);
            }
            catch (JsonException)
            {
                return StatusCode(500, "Failed to parse detail response");
            }

            var breed = detail?.Breeds?.FirstOrDefault();

            var dto = new CatDto
            {
                Id = first.Id,
                ImageUrl = first.Url ?? detail?.Url ?? string.Empty,
                BreedName = breed?.Name ?? "Unknown",
                BreedDescription = breed?.Description ?? string.Empty
            };

            return Ok(dto);
        }

        // Helper types for deserialization (shallow)
        private class SearchResult
        {
            public string? Id { get; set; }
            public string? Url { get; set; }
        }

        private class ImageDetail
        {
            public string? Id { get; set; }
            public string? Url { get; set; }
            public List<ApiBreed>? Breeds { get; set; }
        }

        private class ApiBreed
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
        }
    }
}