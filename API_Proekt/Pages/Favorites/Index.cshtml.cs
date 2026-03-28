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

        // On GET: Проверява дали има бисквитки
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

        // Authenticate form; Форма за автентикация
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
                
            // Създаваме бисквитки да пази кой е влязъл.

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,                      
                Secure = Request.IsHttps,             
                SameSite = SameSiteMode.Lax

            };
            Response.Cookies.Append(AuthCookieName, user.Id.ToString(), cookieOptions);


            AuthenticatedUsername = user.Username;
            Favorite = await _context.Favorites
                .Where(f => f.UserId == user.Id)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();


            FormPassword = null;

            return Page();
        }

        // Форма за да чисти потребителя
        [ValidateAntiForgeryToken]
        public IActionResult OnPostClearAuth()
        {
            Response.Cookies.Delete(AuthCookieName);
            return RedirectToPage(); 
        }
    }
}
