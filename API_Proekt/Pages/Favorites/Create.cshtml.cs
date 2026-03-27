using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using API_Proekt.Data;
using API_Proekt.Models;

namespace API_Proekt.Pages.Favorites
{
    public class CreateModel : PageModel
    {
        private readonly API_Proekt.Data.AppDbContext _context;

        public CreateModel(API_Proekt.Data.AppDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public Favorite Favorite { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Favorites.Add(Favorite);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
