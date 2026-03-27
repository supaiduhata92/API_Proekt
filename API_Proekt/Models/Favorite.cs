namespace API_Proekt.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        // Foreign key
        public int UserId { get; set; }
        public User? User { get; set; }

        public string AnimalType { get; set; } // "Dog" or "Cat"
        public string Breed { get; set; }
        public string ImageUrl { get; set; }
        public string Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
