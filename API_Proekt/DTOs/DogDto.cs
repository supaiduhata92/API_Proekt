namespace API_Proekt.DTOs
{
    public class DogDto
    {
        public string Id { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string BreedName { get; set; } = "Unknown";
        public string BreedDescription { get; set; } = string.Empty;
    }
}