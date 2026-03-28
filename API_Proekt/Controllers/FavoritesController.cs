using API_Proekt.Data;
using API_Proekt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API_Proekt.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoritesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            //var favorites = await _context.Favorites
            //    .Include(f => f.User)
            //    .ToListAsync();
            //return Ok(favorites);
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int currentUserId = int.Parse(userId);

            var favorites = await _context.Favorites
                .Where(f => f.UserId == currentUserId)
                .Select(f => new
                {
                    f.Id,
                    f.AnimalType,
                    f.Breed
                })
                .ToListAsync();

            return Ok(favorites);
        }

        [HttpPost]
        public async Task<IActionResult> AddFavorite([FromBody] Favorite model)
        {

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            model.UserId = userId;
            model.CreatedAt = DateTime.Now;

            _context.Favorites.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFavorite(int id, UpdateFavoriteDto model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
            if (favorite == null)
                return NotFound("Favorite not found");

            if (model.AnimalType != null)
                favorite.AnimalType = model.AnimalType;

            if (model.Breed != null)
                favorite.Breed = model.Breed;

            if (model.ImageUrl != null)
                favorite.ImageUrl = model.ImageUrl;
            favorite.CreatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(favorite);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavorite(int id)
        {
            var favorite = await _context.Favorites.FindAsync(id);
            if (favorite == null)
                return NotFound("Favorite not found");

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully" });
        }
    }
}
