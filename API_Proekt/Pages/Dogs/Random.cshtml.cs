using API_Proekt.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text.Json;

namespace API_Proekt.Pages.Dogs
{
    public class RandomModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string SearchUrl = "https://api.thedogapi.com/v1/images/search?limit=1&has_breeds=1";
        private const string ImageByIdUrlTemplate = "https://api.thedogapi.com/v1/images/{0}";

        public RandomModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public DogDto? Dog { get; set; }


        public void OnGet()
        {
        }


        public async Task<IActionResult> OnPostFetchAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // 1) find an image that has breed info
            var searchResp = await client.GetAsync(SearchUrl);
            if (!searchResp.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to fetch dog search data.");
                return Page();
            }

            var searchJson = await searchResp.Content.ReadAsStringAsync();
            List<SearchResult>? searchResults;
            try
            {
                searchResults = JsonSerializer.Deserialize<List<SearchResult>>(searchJson, options);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Failed to parse search response.");
                return Page();
            }

            if (searchResults == null || searchResults.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No dog image returned from search.");
                return Page();
            }

            var first = searchResults[0];
            if (string.IsNullOrEmpty(first?.Id))
            {
                ModelState.AddModelError(string.Empty, "Search result did not include an id.");
                return Page();
            }

            // Взимаме и детайлите за изображението, за да получим информация за породата
            var detailResp = await client.GetAsync(string.Format(ImageByIdUrlTemplate, first.Id));
            if (!detailResp.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to fetch dog details.");
                return Page();
            }

            var detailJson = await detailResp.Content.ReadAsStringAsync();
            ImageDetail? detail;
            try
            {
                detail = JsonSerializer.Deserialize<ImageDetail>(detailJson, options);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Failed to parse detail response.");
                return Page();
            }

            var breed = detail?.Breeds?.FirstOrDefault();

            Dog = new DogDto
            {
                Id = first.Id,
                ImageUrl = first.Url ?? detail?.Url ?? string.Empty,
                BreedName = breed?.Name ?? "Unknown",
                BreedDescription = breed?.Temperament ?? breed?.BredFor ?? breed?.LifeSpan ?? breed?.Origin ?? breed?.Name ?? string.Empty
            };


            if (breed != null && !string.IsNullOrWhiteSpace(breed?.Name))
            {
                // Комбинираме полета за описание, ако Description е празно
                Dog.BreedDescription = string.IsNullOrWhiteSpace(breed.Description)
                    ? $"{(breed.Temperament ?? "").Trim()} {(breed.BredFor ?? "").Trim()}".Trim()
                    : breed.Description;
            }

            return Page();
        }


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
            public string? Temperament { get; set; }
            public string? BredFor { get; set; }
            public string? LifeSpan { get; set; }
            public string? Origin { get; set; }
        }
    }
}