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
    [Authorize(Roles = "Author")]
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

        //Book endpoints

        [HttpPost("publish")]

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
                                       // Comments, CriticRatings, ReaderRatings, Chapters kihagyása
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

                var favoritesToRemove = await _dbContext.Favourites
                      .Where(fav => fav.BookId == bookId)
                      .ToListAsync();

                _dbContext.Favourites.RemoveRange(favoritesToRemove);
                await _dbContext.SaveChangesAsync();

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

        //Chapter endpoints

        [HttpGet("chapters/{bookId}")]
        public async Task<IActionResult> GetChaptersByBook(int bookId)
        {
            var chapters = await _dbContext.Chapters
                .Where(c => c.BookId == bookId)
                .OrderBy(c => c.ChapterNumber)
                .ToListAsync();

            if (chapters == null) return NotFound();

            return Ok(chapters);
        }

        [HttpGet("chapter/{bookId}/{chapterNumber}")]
        public async Task<IActionResult> GetSpecificChapter(int bookId, int chapterNumber)
        {
            var chapter = await _dbContext.Chapters
                .Where(c => c.BookId == bookId && c.ChapterNumber == chapterNumber)
                .FirstOrDefaultAsync();

            if (chapter == null) return NotFound();

            return Ok(chapter);
        }
        [HttpPost("publishChapter")]
        public async Task<IActionResult> AddChapter([FromBody] ChapterDTO chapterDto)
        {
            // Retrieve the book using BookId from chapterDto
            var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.BookId == chapterDto.BookId);
            if (book == null)
            {
                return BadRequest("The specified book does not exist.");
            }

            // Retrieve the current user's ID from claims
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User is not authenticated.");
            }

            // Check if the current user is the author of the book
            if (book.AuthorId != currentUserId)
            {
                return Unauthorized("You are not authorized to publish a chapter for this book.");
            }

            // Create a new chapter object and populate it with data from chapterDto
            var chapter = new Chapters
            {
                BookId = chapterDto.BookId,
                ChapterNumber = chapterDto.ChapterNumber,
                ChapterTitle = chapterDto.ChapterTitle,
                ChapterContent = chapterDto.ChapterContent
            };

            // Add the new chapter to the database
            _dbContext.Chapters.Add(chapter);
            await _dbContext.SaveChangesAsync();

            // Return a Created response with the newly created chapter
            return CreatedAtAction(nameof(GetSpecificChapter), new { bookId = chapterDto.BookId, chapterNumber = chapterDto.ChapterNumber }, chapter);
        }




        [HttpPut("modifyChapter/{chapterId}")]
        public async Task<IActionResult> UpdateChapter(int chapterId, [FromBody] ChapterDTO chapterDto)
        {
            var chapter = await _dbContext.Chapters.FindAsync(chapterId);
            if (chapter == null)
            {
                return NotFound("The specified chapter does not exist.");
            }

            var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.BookId == chapterDto.BookId);
            if (book == null)
            {
                return BadRequest("The specified book does not exist.");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User is not authenticated.");
            }

            if (book.AuthorId != currentUserId)
            {
                return Unauthorized("You are not authorized to modify this chapter.");
            }

            chapter.ChapterNumber = chapterDto.ChapterNumber;
            chapter.ChapterTitle = chapterDto.ChapterTitle;
            chapter.ChapterContent = chapterDto.ChapterContent;

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }



        [HttpDelete("deleteChapter/{chapterId}")]
        public async Task<IActionResult> DeleteChapter(int chapterId)
        {
            var chapter = await _dbContext.Chapters.FindAsync(chapterId);
            if (chapter == null)
            {
                return NotFound("The specified chapter does not exist.");
            }

            var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.BookId == chapter.BookId);
            if (book == null)
            {
                return BadRequest("The specified book does not exist.");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User is not authenticated.");
            }
            if (book.AuthorId != currentUserId)
            {
                return Unauthorized("You are not authorized to delete this chapter.");
            }
            _dbContext.Chapters.Remove(chapter);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }





    }
}
