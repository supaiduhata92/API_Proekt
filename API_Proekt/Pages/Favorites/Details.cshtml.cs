using System;
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
        private readonly AppDbContext _context;

        public DetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public Favorite? Favorite { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            Favorite = await _context.Favorites
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id.Value);

            if (Favorite == null)
                return NotFound();

            return Page();
        }
    }
}
