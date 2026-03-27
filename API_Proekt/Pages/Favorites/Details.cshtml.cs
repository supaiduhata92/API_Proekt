using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using API_Proekt.Data;
using API_Proekt.Models;

namespace API_Proekt.Pages.Favorites
{
    public class DetailsModel : PageModel
    {
        private readonly API_Proekt.Data.AppDbContext _context;

        public DetailsModel(API_Proekt.Data.AppDbContext context)
        {
            _context = context;
        }

        public Favorite Favorite { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var favorite = await _context.Favorites.FirstOrDefaultAsync(m => m.Id == id);

            if (favorite is not null)
            {
                Favorite = favorite;

                return Page();
            }

            return NotFound();
        }
    }
}
