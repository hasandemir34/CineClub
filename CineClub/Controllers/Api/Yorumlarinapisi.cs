using CineClub.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace CineClub.Controllers.Api;

[ApiController]
[Route("api/reviews")]
public class Yorumlarinapisi : ControllerBase
{
    private readonly CineDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public Yorumlarinapisi(CineDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ÖZELLIK 4: GET: /api/reviews/{movieId}
    [HttpGet("{movieId}")]
    public async Task<IActionResult> GetMovieReviews(int movieId)
    {
        var movie = await _context.Movies.FindAsync(movieId);
        if (movie == null)
        {
            return NotFound(new { message = "Movie not found" });
        }

        var reviews = await _context.Reviews
            .Where(r => r.MovieId == movieId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync();

        var reviewsWithUsernames = new List<object>();

        foreach (var review in reviews)
        {
            string? username = null;
            if (!string.IsNullOrEmpty(review.UserId))
            {
                var user = await _userManager.FindByIdAsync(review.UserId);
                username = user?.UserName;
            }

            reviewsWithUsernames.Add(new
            {
                review = review.Content,
                reviewRate = review.Rating,
                reviewDate = review.CreatedAtUtc,
                username = username ?? "Unknown"
            });
        }

        return Ok(reviewsWithUsernames);
    }
}