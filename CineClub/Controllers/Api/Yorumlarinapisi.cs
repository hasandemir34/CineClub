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
    private readonly TimeZoneInfo _turkeyTimeZone;

    public Yorumlarinapisi(CineDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
        
        // Türkiye saat dilimini ayarla
        try
        {
            _turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        }
        catch
        {
            // Linux/Mac için alternatif
            _turkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        }
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

            // Görüntülenecek en güncel tarihi seç: UpdatedAtUtc varsa onu, yoksa CreatedAtUtc'yi kullan.
            var displayDate = review.UpdatedAtUtc ?? review.CreatedAtUtc;
            var isEdited = review.UpdatedAtUtc.HasValue;

            DateTime? reviewDateTurkey = null;

            if (displayDate.HasValue)
            {
                // DÜZELTME: Kullanıcının bildirdiği (+3 saat ilerisi) hatayı çözmek için.
                // Stored time'ın hatalı bir şekilde zaten Türkiye yerel saati olarak kaydedildiği varsayılırsa,
                // fazladan bir UTC çevrimi yapmaktan kaçınıyoruz.
                // Bu, ConvertTimeFromUtc çağrısını kaldırarak hatayı doğrudan çözer.
                reviewDateTurkey = displayDate.Value;
            }

            reviewsWithUsernames.Add(new
            {
                review = review.Content,
                reviewRate = review.Rating,
                // Artık fazladan 3 saat eklenmeden, en son düzenleme zamanı (veya oluşturulma) doğru saatte görüntülenir.
                reviewDate = reviewDateTurkey?.ToString("dd.MM.yyyy HH:mm"),
                reviewDateUtc = review.CreatedAtUtc, 
                reviewUpdateDateUtc = review.UpdatedAtUtc, 
                isEdited = isEdited, 
                username = username ?? "Unknown"
            });
        }

        return Ok(reviewsWithUsernames);
    }
}