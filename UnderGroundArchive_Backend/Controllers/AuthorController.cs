using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UnderGroundArchive_Backend.Dbcontext;
using UnderGroundArchive_Backend.DTO;
using UnderGroundArchive_Backend.Models;
using UnderGroundArchive_Backend.Services;

namespace UnderGroundArchive_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UGA_DBContext _dbContext;

        public AuthorController(IBookService bookService, UserManager<ApplicationUser> userManager, UGA_DBContext dBContext)
        {
            _bookService = bookService;
            _userManager = userManager;
            _dbContext = dBContext;
        }

        [HttpPost("publish")]
        [Authorize(Roles = "Author")]
        public async Task<IActionResult> PublishBook([FromBody] PublishBookDTO bookDto)
        {
            if (bookDto == null || string.IsNullOrEmpty(bookDto.BookName) || bookDto.GenreId <= 0 || bookDto.CategoryId <= 0)
            {
                return BadRequest(new { message = "Hiányos vagy érvénytelen adatokat küldtél." });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Nem található bejelentkezett felhasználó. A token érvénytelen vagy hiányzik." });
            }

            try
            {
                var book = new Books
                {
                    BookName = bookDto.BookName,
                    GenreId = bookDto.GenreId,
                    CategoryId = bookDto.CategoryId,
                    BookDescription = bookDto.BookDescription,
                    AuthorId = userId, // JWT-ből vesszük az AuthorId-t
                                       // Comments, CriticRatings, ReaderRatings kihagyása
                };

                _dbContext.Books.Add(book);
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "A könyv sikeresen publikálva.", bookId = book.BookId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Belső szerverhiba történt a könyv mentése során.", error = ex.Message });
            }


        }

        [HttpPut("modify/{bookId}")]
        [Authorize(Roles = "Author")]
        public async Task<IActionResult> ModifyBook(int bookId, [FromBody] ModifyBookDTO bookDto)
        {
            if (bookDto == null || string.IsNullOrEmpty(bookDto.BookName) || bookDto.GenreId <= 0 || bookDto.CategoryId <= 0)
            {
                return BadRequest(new { message = "Hiányos vagy érvénytelen adatokat küldtél." });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Nem található bejelentkezett felhasználó. A token érvénytelen vagy hiányzik." });
            }

            try
            {
                // Fetch the book to be modified from the database
                var book = await _dbContext.Books
                    .FirstOrDefaultAsync(b => b.BookId == bookId && b.AuthorId == userId);

                if (book == null)
                {
                    return NotFound(new { message = "A könyvet nem találjuk vagy nem jogosult a módosításra." });
                }

                // Update the book details
                book.BookName = bookDto.BookName;
                book.GenreId = bookDto.GenreId;
                book.CategoryId = bookDto.CategoryId;
                book.BookDescription = bookDto.BookDescription;

                // Save changes to the database
                _dbContext.Books.Update(book);
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "A könyv sikeresen módosítva.", bookId = book.BookId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Belső szerverhiba történt a könyv módosítása során.", error = ex.Message });
            }
        }


        [HttpDelete("delete/{bookId}")]
        [Authorize(Roles = "Author")]
        public async Task<IActionResult> DeleteBook(int bookId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Nem található bejelentkezett felhasználó. A token érvénytelen vagy hiányzik." });
            }

            try
            {
                // Fetch the book to be deleted from the database
                var book = await _dbContext.Books
                    .FirstOrDefaultAsync(b => b.BookId == bookId && b.AuthorId == userId);

                if (book == null)
                {
                    return NotFound(new { message = "A könyvet nem találjuk vagy nem jogosult a törlésére." });
                }

                // Delete related comments
                var comments = await _dbContext.Comments
                    .Where(c => c.BookId == bookId)
                    .ToListAsync();
                _dbContext.Comments.RemoveRange(comments);

                // Delete related reader ratings
                var readerRatings = await _dbContext.ReaderRatings
                    .Where(r => r.BookId == bookId)
                    .ToListAsync();
                _dbContext.ReaderRatings.RemoveRange(readerRatings);

                // Delete related critic ratings
                var criticRatings = await _dbContext.CriticRatings
                    .Where(r => r.BookId == bookId)
                    .ToListAsync();
                _dbContext.CriticRatings.RemoveRange(criticRatings);

                // Remove the book from the favorites list of all users
                var usersWithFavorite = await _dbContext.Users
                    .Where(u => u.Favourites.Contains(bookId.ToString()))
                    .ToListAsync();

                foreach (var user in usersWithFavorite)
                {
                    // Remove the bookId from the Favourites string (if it exists)
                    var favouritesList = user.Favourites.Split(',').Where(fav => fav != bookId.ToString()).ToList();
                    user.Favourites = string.Join(",", favouritesList);

                    _dbContext.Users.Update(user); // Update the user record
                }

                // Now delete the book
                _dbContext.Books.Remove(book);
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "A könyv és annak kapcsolódó adatainak törlése sikeresen megtörtént." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Belső szerverhiba történt a könyv törlése során.", error = ex.Message });
            }
        }


    }
}
