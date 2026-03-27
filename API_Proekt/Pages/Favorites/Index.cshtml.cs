using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using API_Proekt.Data;
using API_Proekt.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace API_Proekt.Pages.Favorites
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private const string AuthCookieName = "favorites_auth";

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Favorite> Favorite { get; set; } = new List<Favorite>();

        public string? AuthenticatedUsername { get; set; }

        [BindProperty]
        public string? FormUsername { get; set; }

        [BindProperty, DataType(DataType.Password)]
        public string? FormPassword { get; set; }

        // On GET: if cookie exists, auto-load that user's favorites
        public async Task OnGetAsync()
        {
            if (Request.Cookies.TryGetValue(AuthCookieName, out var userIdValue)
                && int.TryParse(userIdValue, out var userId))
            {
                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    AuthenticatedUsername = user.Username;
                    Favorite = await _context.Favorites
                        .Where(f => f.UserId == user.Id)
                        .OrderByDescending(f => f.CreatedAt)
                        .ToListAsync();
                }
            }
        }

        // Authenticate form; if successful set cookie and load favorites
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAuthenticateAsync()
        {
            if (string.IsNullOrWhiteSpace(FormUsername) || string.IsNullOrWhiteSpace(FormPassword))
            {
                ModelState.AddModelError(string.Empty, "Enter both username and password.");
                return Page();
            }

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == FormUsername);

            if (user == null || !BCrypt.Net.BCrypt.Verify(FormPassword, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return Page();
            }
                
            // Set a session cookie with the user id so OnGet can auto-load favorites.
            // Session cookie: do NOT set Expires — it will be cleared when the browser/process closes.
            // Use HttpOnly=true so the cookie isn't accessible from JS (safer).
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,                      // prevent JS access
                Secure = Request.IsHttps,             // set secure on HTTPS
                SameSite = SameSiteMode.Lax
                // no Expires => session cookie (deleted when browser closes)
            };
            Response.Cookies.Append(AuthCookieName, user.Id.ToString(), cookieOptions);

            // Load favorites for this user
            AuthenticatedUsername = user.Username;
            Favorite = await _context.Favorites
                .Where(f => f.UserId == user.Id)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            // clear the posted password from memory
            FormPassword = null;

            return Page();
        }

        // Clear the auth cookie (so page will ask again)
        [ValidateAntiForgeryToken]
        public IActionResult OnPostClearAuth()
        {
            Response.Cookies.Delete(AuthCookieName);
            return RedirectToPage(); // GET will show unauthenticated form
        }
    }
}
