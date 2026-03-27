using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using API_Proekt.Data;
using API_Proekt.Models;
using System.ComponentModel.DataAnnotations;

namespace API_Proekt.Pages.Favorites
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        // Favorites that will be shown after successful "authentication" on the page
        public IList<Favorite> Favorite { get; set; } = new List<Favorite>();

        // Show which user we're showing favorites for (null when not authenticated)
        public string? AuthenticatedUsername { get; set; }

        // Form fields bound from the username/password form
        [BindProperty]
        public string? FormUsername { get; set; }

        [BindProperty, DataType(DataType.Password)]
        public string? FormPassword { get; set; }

        // Keep GET simple; do not load all favorites by default
        public void OnGet()
        {
        }

        // Handler: post username + password; if valid load only that user's favorites
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAuthenticateAsync()
        {
            if (string.IsNullOrWhiteSpace(FormUsername) || string.IsNullOrWhiteSpace(FormPassword))
            {
                ModelState.AddModelError(string.Empty, "Enter both username and password.");
                return Page();
            }

            // Find user
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == FormUsername);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return Page();
            }

            // Verify password (plaintext, hashed in DB)
            if (!BCrypt.Net.BCrypt.Verify(FormPassword, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return Page();
            }

            // Auth successful — load only this user's favorites
            AuthenticatedUsername = user.Username;

            Favorite = await _context.Favorites
                .Where(f => f.UserId == user.Id)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            // Clear bound password for safety
            FormPassword = null;

            return Page();
        }
    }
}
