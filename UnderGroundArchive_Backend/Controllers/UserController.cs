using Microsoft.AspNetCore.Mvc;
using UnderGroundArchive_Backend.Dbcontext;
using Microsoft.EntityFrameworkCore;
using UnderGroundArchive_Backend.Models;
using UnderGroundArchive_Backend.DTO;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace UnderGroundArchive_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UGA_DBContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager, UGA_DBContext dBContext)
        {
            _dbContext = dBContext;
            _userManager = userManager;
        }
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _dbContext.Users.ToListAsync();
            return Ok(users);
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            return Ok(user);
        }

        //test:
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            // A tokenből kinyert felhasználói ID (a JWT-ban található)
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized(); // Ha nincs ID, akkor 401-es válasz
            }

            // Felhasználó lekérése az ID alapján
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound(); // Ha nincs ilyen felhasználó, akkor 404
            }

            return Ok(user); // A felhasználó adatainak visszaadása
        }


        [HttpGet("books")]
        public async Task<ActionResult<IEnumerable<Books>>> GetBooks()
        {
            var books = await _dbContext.Books.Include(c => c.Comments).Include(r => r.ReaderRatings).Include(cr => cr.CriticRatings).ToListAsync();
            return Ok(books);
        }


        [HttpGet("book/{id}")]
        public async Task<ActionResult<Books>> GetBook(int id)
        {
            var book = await _dbContext.Books.Include(c => c.Comments).Include(r => r.ReaderRatings).Include(cr => cr.CriticRatings).FirstOrDefaultAsync(c => c.BookId == id);
            return book == null ? NotFound() : book;
        }

        // Request endpoints

        [HttpGet("requests")]
        public async Task<ActionResult<IEnumerable<Requests>>> GetRequests()
        {


            return await _dbContext.Requests.ToListAsync();
        }

        [HttpGet("request/{id}")]
        public async Task<ActionResult<Requests>> GetRequest(int id)
        {
            var request = await _dbContext.Requests.FirstOrDefaultAsync(j => j.RequestId == id);
            return request == null ? NotFound() : request;
        }

        [HttpPost("createRequest")]
        public async Task<IActionResult> CreateRequest(Requests request)
        {
            if (request.RequestType <= 0)
            {
                return BadRequest("Invalid RequestType specified.");
            }

            if (string.IsNullOrEmpty(request.RequesterId))
            {
                return BadRequest("RequesterId is required.");
            }

 
            _dbContext.Requests.Add(request);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction("GetRequest", new { id = request.RequestId }, request);
        }

        // ReaderRating endpoints

        [HttpGet("readerRatings")]
        public async Task<ActionResult<IEnumerable<ReaderRatings>>> GetReaderRatings()
        {


            return await _dbContext.ReaderRatings.ToListAsync();
        }

        [HttpGet("readerRating/{id}")]
        public async Task<ActionResult<ReaderRatings>> GetReaderRating(int id)
        {
            var readerRating = await _dbContext.ReaderRatings.Include(j => j.Books).FirstOrDefaultAsync(j => j.RatingId == id);
            return readerRating == null ? NotFound() : readerRating;
        }

        [HttpPost("createReaderRating")]
        public async Task<IActionResult> CreateReaderRating(ReaderRatings readerRating)
        {
            var bookExists = await _dbContext.ReaderRatings.AnyAsync(k => k.BookId == readerRating.BookId);
            if (!bookExists)
            {
                return BadRequest("The specified Book does not exist.");
            }

            _dbContext.ReaderRatings.Add(readerRating);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction("GetReaderRating", new { id = readerRating.RatingId }, readerRating);
        }

        [HttpPut("modifyReaderRating/{id}")]
        public async Task<ActionResult> PutReaderRating(int id, ReaderRatings modifiedReaderRating)
        {
            if (id != modifiedReaderRating.RatingId)
            {
                return BadRequest();
            }

            modifiedReaderRating.ModifiedAt = DateTime.Now;
            _dbContext.Entry(modifiedReaderRating).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("deleteReaderRating/{id}")]
        public async Task<ActionResult> DeleteReaderRating(int id)
        {
            var readerRating = await _dbContext.ReaderRatings.FindAsync(id);
            if (readerRating == null)
            {
                return NotFound();
            }

            _dbContext.ReaderRatings.Remove(readerRating);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        // ReaderRating endpoints

        [HttpGet("criticRatings")]
        public async Task<ActionResult<IEnumerable<CriticRatings>>> GetCriticRatings()
        {


            return await _dbContext.CriticRatings.ToListAsync();
        }

        [HttpGet("criticRating/{id}")]
        public async Task<ActionResult<CriticRatings>> GetCriticRating(int id)
        {
            var criticRating = await _dbContext.CriticRatings.Include(j => j.Books).FirstOrDefaultAsync(j => j.RatingId == id);
            return criticRating == null ? NotFound() : criticRating;
        }

        [HttpPost("createCriticRating")]
        public async Task<IActionResult> CreateCriticRating(CriticRatings criticRating)
        {
            var bookExists = await _dbContext.CriticRatings.AnyAsync(k => k.BookId == criticRating.BookId);
            if (!bookExists)
            {
                return BadRequest("The specified Book does not exist.");
            }

            _dbContext.CriticRatings.Add(criticRating);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction("GetCriticRating", new { id = criticRating.RatingId }, criticRating);
        }

        [HttpPut("modifyCriticRating/{id}")]
        public async Task<ActionResult> PutCriticRating(int id, CriticRatings modifiedCriticRating)
        {
            if (id != modifiedCriticRating.RatingId)
            {
                return BadRequest();
            }

            modifiedCriticRating.ModifiedAt = DateTime.Now;
            _dbContext.Entry(modifiedCriticRating).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("deleteCriticRating/{id}")]
        public async Task<ActionResult> DeleteCriticRating(int id)
        {
            var criticRating = await _dbContext.CriticRatings.FindAsync(id);
            if (criticRating == null)
            {
                return NotFound();
            }

            _dbContext.CriticRatings.Remove(criticRating);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        // Comment endpoints
        [HttpGet("comments")]
        public async Task<ActionResult<IEnumerable<Comments>>> GetComments()
        {


            return await _dbContext.Comments.ToListAsync();
        }

        [HttpGet("comment/{id}")]
        public async Task<ActionResult<Comments>> GetComment(int id)
        {
            var comment = await _dbContext.Comments.Include(j => j.Books).FirstOrDefaultAsync(j => j.CommentId == id);
            return comment == null ? NotFound() : comment;
        }

        [HttpPost("createComment")]
        public async Task<IActionResult> CreateComment(Comments comment)
        {
            var bookExists = await _dbContext.Books.AnyAsync(k => k.BookId == comment.BookId);
            if (!bookExists)
            {
                return BadRequest("The specified Book does not exist.");
            }

            _dbContext.Comments.Add(comment);
            await _dbContext.SaveChangesAsync();

            return CreatedAtAction("GetComment", new { id = comment.CommentId }, comment);
        }

        [HttpPut("modifyComment/{id}")]
        public async Task<ActionResult> PutComment(int id, Comments modifiedComment)
        {
            if (id != modifiedComment.CommentId)
            {
                return BadRequest();
            }
            
            modifiedComment.ModifiedAt = DateTime.Now;
            _dbContext.Entry(modifiedComment).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("deleteComment/{id}")]
        public async Task<ActionResult> DeleteComment(int id)
        {
            var comment = await _dbContext.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            _dbContext.Comments.Remove(comment);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        // Favorite endpoints


        [HttpGet("favorites")]
        public async Task<IActionResult> GetFavorites()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Retrieve the favorites from the user's Favourites property (stored as a comma-separated string)
            var favoriteIds = user.Favourites?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            var favorites = await _dbContext.Books.Where(book => favoriteIds.Contains(book.BookId.ToString())).ToListAsync();

            return Ok(favorites);
        }

        [HttpPost("addFavorite/{bookId}")]
        public async Task<IActionResult> AddFavorite(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var bookExists = await _dbContext.Books.AnyAsync(b => b.BookId == bookId);
            if (!bookExists)
            {
                return BadRequest("The specified Book does not exist.");
            }

            // Add the book ID to the user's Favourites property
            var favoriteIds = user.Favourites?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            if (favoriteIds.Contains(bookId.ToString()))
            {
                return BadRequest("This book is already in your favorites.");
            }

            favoriteIds.Add(bookId.ToString());
            user.Favourites = string.Join(",", favoriteIds);

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            return Ok("Book added to favorites.");
        }

        [HttpDelete("removeFavorite/{bookId}")]
        public async Task<IActionResult> RemoveFavorite(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var favoriteIds = user.Favourites?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            if (!favoriteIds.Contains(bookId.ToString()))
            {
                return BadRequest("This book is not in your favorites.");
            }

            favoriteIds.Remove(bookId.ToString());
            user.Favourites = string.Join(",", favoriteIds);

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            return Ok("Book removed from favorites.");
        }

        [HttpPut("updateFavorites")]
        public async Task<IActionResult> UpdateFavorites([FromBody] List<int> bookIds)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Validate that all provided book IDs exist
            var validBooks = await _dbContext.Books
                .Where(book => bookIds.Contains(book.BookId))
                .Select(book => book.BookId)
                .ToListAsync();

            if (validBooks.Count != bookIds.Count)
            {
                return BadRequest("Some provided book IDs are invalid.");
            }

            // Update the user's Favourites property
            user.Favourites = string.Join(",", bookIds);
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            return Ok("Favorites updated successfully.");
        }


    }
}
