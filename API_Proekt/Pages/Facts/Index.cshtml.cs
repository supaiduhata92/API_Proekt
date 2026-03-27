using API_Proekt.DTOs;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace API_Proekt.Pages.Facts
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public FactDto? CatFact { get; set; }
        public FactDto? DogFact { get; set; }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();

            // Cat
            try
            {
                using var catResp = await client.GetAsync("https://catfact.ninja/fact");
                if (catResp.IsSuccessStatusCode)
                {
                    var json = await catResp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                        doc.RootElement.TryGetProperty("fact", out var factEl))
                    {
                        CatFact = new FactDto { Fact = factEl.GetString() ?? string.Empty };
                    }
                }
            }
            catch
            {
                // keep CatFact null on error
            }

            // Dog
            try
            {
                using var dogResp = await client.GetAsync("https://dogapi.dog/api/v1/facts?number=1");
                if (dogResp.IsSuccessStatusCode)
                {
                    var json = await dogResp.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    // try common shapes
                    if (root.TryGetProperty("facts", out var factsEl) && factsEl.ValueKind == JsonValueKind.Array && factsEl.GetArrayLength() > 0)
                    {
                        var first = factsEl[0];
                        if (first.ValueKind == JsonValueKind.String)
                            DogFact = new FactDto { Fact = first.GetString() ?? string.Empty };
                        else if (first.ValueKind == JsonValueKind.Object && first.TryGetProperty("fact", out var nested))
                            DogFact = new FactDto { Fact = nested.GetString() ?? string.Empty };
                    }
                    else if (root.TryGetProperty("fact", out var factProp) && factProp.ValueKind == JsonValueKind.String)
                    {
                        DogFact = new FactDto { Fact = factProp.GetString() ?? string.Empty };
                    }
                    else if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0 && root[0].ValueKind == JsonValueKind.String)
                    {
                        DogFact = new FactDto { Fact = root[0].GetString() ?? string.Empty };
                    }
                }
            }
            catch
            {
                // keep DogFact null on error
            }
        }
    }
}
