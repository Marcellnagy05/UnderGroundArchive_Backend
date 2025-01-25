using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UnderGroundArchive_Backend.Dbcontext;
using UnderGroundArchive_Backend.DTO;
using UnderGroundArchive_Backend.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace UnderGroundArchive_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UGA_DBContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager, UGA_DBContext dBContext)
        {
            _dbContext = dBContext;
            _userManager = userManager;
        }

        //get user endpoints

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _dbContext.Users
                .Select(user => new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.JoinDate,
                    user.BirthDate,
                    user.Country,
                    user.RankPoints,
                    user.Balance,
                    user.IsMuted,
                    user.IsBanned,
                    RoleName = _dbContext.UserRoles
                        .Where(ur => ur.UserId == user.Id)
                        .Join(_dbContext.Roles,
                              ur => ur.RoleId,
                              r => r.Id,
                              (ur, r) => r.Name)
                        .FirstOrDefault(), // Fetch the first role name (if users can only have one role)
                    RankName = _dbContext.Ranks
                        .Where(r => r.RankId == user.RankId)
                        .Select(r => r.RankName)
                        .FirstOrDefault(),
                    SubscriptionName = _dbContext.Subscription
                        .Where(s => s.SubscriptionId == user.SubscriptionId)
                        .Select(s => s.SubscriptionName)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(users);
        }
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _dbContext.Users
         .Where(u => u.Id == id)
        .Select(user => new
       {
           user.Id,
           user.UserName,
           user.Email,
           user.JoinDate,
           user.BirthDate,
           user.Country,
           user.RankPoints,
           user.Balance,
           user.IsMuted,
           user.IsBanned,
           RoleName = _dbContext.UserRoles
                        .Where(ur => ur.UserId == user.Id)
                        .Join(_dbContext.Roles,
                              ur => ur.RoleId,
                              r => r.Id,
                              (ur, r) => r.Name)
                        .FirstOrDefault(), // Fetch the first role name (if users can only have one role)
           RankName = _dbContext.Ranks
                        .Where(r => r.RankId == user.RankId)
                        .Select(r => r.RankName)
                        .FirstOrDefault(),
           SubscriptionName = _dbContext.Subscription
                        .Where(s => s.SubscriptionId == user.SubscriptionId)
                        .Select(s => s.SubscriptionName)
                        .FirstOrDefault()
                })
                .ToListAsync();

            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }


            return Ok(user);
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

        //role change endpoint

        [HttpPut("user/{id}/role/{newRoleName}")]
        public async Task<IActionResult> UpdateUserRole(string id, string newRoleName)
        {
            // Validate the new role name
            if (string.IsNullOrWhiteSpace(newRoleName))
            {
                return BadRequest(new { Message = "Role name cannot be empty" });
            }

            // Find the user by ID
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            // Find the role by name (case-insensitive)
            var role = await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == newRoleName.ToLower());

            if (role == null)
            {
                return NotFound(new { Message = "Role not found" });
            }

            // Remove any existing roles for the user
            var userRoles = await _dbContext.UserRoles
                .Where(ur => ur.UserId == id)
                .ToListAsync();

            _dbContext.UserRoles.RemoveRange(userRoles);

            // Add the new role to the user
            var userRole = new IdentityUserRole<string>
            {
                UserId = id,
                RoleId = role.Id
            };
            await _dbContext.UserRoles.AddAsync(userRole);

            // Save changes to the database
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "User role updated successfully" });
        }



    }
}
