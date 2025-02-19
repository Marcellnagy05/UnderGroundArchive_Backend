using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mysqlx;
using System.Security.Claims;
using UnderGroundArchive_Backend.Dbcontext;
using UnderGroundArchive_Backend.DTO;
using UnderGroundArchive_Backend.Models;

namespace UnderGroundArchive_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Moderator")]
    public class ModeratorController : ControllerBase
    {
        private readonly UGA_DBContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public ModeratorController(UserManager<ApplicationUser> userManager, UGA_DBContext dBContext)
        {
            _dbContext = dBContext;
            _userManager = userManager;
        }



        //report endpoints

        [HttpGet("reports")]
        public async Task<IActionResult> GetAllReports()
        {
            var reports = await _dbContext.Reports
                .Select(r => new
                {
                    r.ReportId,
                    r.ReporterId,
                    r.ReportedPersonId,
                    r.ReportedCommentId,
                    r.ReportedBookId,
                    r.ReportTypeId,
                    r.ReportMessage,
                    r.IsHandled,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(reports);
        }

        [HttpGet("pendingReports")]
        public async Task<IActionResult> GetPendingReports()
        {
            var reports = await _dbContext.Reports
                .Where(x => x.IsHandled == false)
                .Select(r => new
                {
                    r.ReportId,
                    r.ReporterId,
                    r.ReportedPersonId,
                    r.ReportedCommentId,
                    r.ReportedBookId,
                    r.ReportTypeId,
                    r.ReportMessage,
                    r.IsHandled,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(reports);
        }


        [HttpGet("report/{id}")]
        public async Task<IActionResult> GetReport(int id)
        {
            var report = await _dbContext.Reports
                .Where(r => r.ReportId == id)
                .Select(r => new
                {
                    r.ReportId,
                    r.ReporterId,
                    r.ReportedPersonId,
                    r.ReportedCommentId,
                    r.ReportedBookId,
                    r.ReportTypeId,
                    r.ReportMessage,
                    r.IsHandled,
                    r.CreatedAt
                })
                .FirstOrDefaultAsync();

            return report == null ? NotFound("Report not found.") : Ok(report);
        }

        [HttpPut("handleReport")]
        public async Task<ActionResult> HandleReport(int reportId)
        {
            var report = await _dbContext.Reports.FirstOrDefaultAsync(r => r.ReportId == reportId);

            if (report == null)
            {
                return NotFound("Report not found.");
            }

            report.IsHandled = true;

            _dbContext.Reports.Update(report);
            await _dbContext.SaveChangesAsync();

            return Ok("Report status updated to handled.");
        }

        //status change endpoints

        [HttpPut("muteStatusChange")]
        public async Task<ActionResult> ChangeMuteStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Felhasználó nem található");
            }
            if (user.IsMuted)
            {
                user.IsMuted = false;
            }
            else
            {
                user.IsMuted = true;
            }


            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok("Mute status changed");
            }
            else
            {
                return BadRequest("Hiba történt");
            }
        }

        [HttpPut("banStatusChanged")]
        public async Task<ActionResult> ChangeBanStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Felhasználó nem található");
            }
            if (user.IsBanned)
            {
                user.IsBanned = false;
            }
            else
            {
                user.IsBanned = true;
            }


            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok("Ban status changed");
            }
            else
            {
                return BadRequest("Hiba történt");
            }
        }

        //deletion of user created content endpoints

        [HttpPut("deleteComment/{id}")]
        public async Task<ActionResult> DeleteComment(int id)
        {
            var comment = await _dbContext.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            comment.CommentMessage = $"Deleted by Moderator";

            _dbContext.Entry(comment).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("deleteBook/{bookId}")]

        public async Task<IActionResult> DeleteBook(int bookId)
        {

            try
            {
                // Fetch the book to be deleted from the database
                var book = await _dbContext.Books
                    .FirstOrDefaultAsync(b => b.BookId == bookId);

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
    }
}
