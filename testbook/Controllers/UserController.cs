using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using testbook.ConfigurationClasses;
using testbook.Data;
using testbook.DTO;
using testbook.Method;
using testbook.ModelData;

namespace testbook.Controllers
{
    [ApiController]
    [Route("[Controller]/[action]")]
    public class UserController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        private readonly Appsetting _appsetting;

        public UserController(ApplicationDbContext context, IOptionsMonitor<Appsetting> optionsMonitor)
        {
            _context = context;
            _appsetting = optionsMonitor.CurrentValue;
        }





        //USERS REGISTRATION
        [HttpPut(Name = "/UsersRegistration")]
        public async Task<IActionResult> UsersRegistration([FromBody] DtoUserInput userinput)
        {
            //Validation UserName
            var usernameexist = await _context.Users.FirstOrDefaultAsync(b => b.UserName == userinput.UserName);
            if (usernameexist != null)
            {
                return BadRequest("UserName already exists");
            }
            //Validation Account
            var accountexist = await _context.Users.FirstOrDefaultAsync(b => b.Account == userinput.Account);
            if (accountexist != null)
            {
                return BadRequest("Account already exists");
            }
            //Bcrypt Password
            var hashpassword = PasswordSecurity.HashPasswords(userinput.Password);



            var newUser = new User
            {
                UserName = userinput.UserName,
                Account = userinput.Account,
                Password = hashpassword,

            };
            _context.Add(newUser);
            await _context.SaveChangesAsync();


            return Ok(new
            {
                Message = $"Sign Up Success Account {newUser.Account} And welcome {newUser.UserName} to KhanhBook ",
                Time = $"Create At: {newUser.CreateAt}"
            });
        }



        //DELETE USERS BY ID
        [Authorize(Policy = "RequireAdminRole")]
        [HttpDelete(Name = "/DeleteIdUser")]
        public async Task<IActionResult> DeleteIdUser(int id)
        {
            var userdelete = await _context.Users.FindAsync(id);

            if (userdelete == null)
            {
                return BadRequest("ID does not exist");
            }


            _context.Users.Remove(userdelete);
            await _context.SaveChangesAsync();

            return Ok(userdelete);
        }

        //DELETE USERS BY ACCOUNT
        //[Authorize(Policy = "RequireAdminRole")]
        [HttpDelete(Name = "/DeleteUser")]
        public async Task<IActionResult> DeleteUser(string account)
        {
            var userdelete = await _context.Users.FirstOrDefaultAsync(b => b.Account == account);

            if (userdelete == null)
            {
                return BadRequest("Account does not exist");
            }


            _context.Users.Remove(userdelete);
            await _context.SaveChangesAsync();

            return Ok(userdelete);
        }



        //EDIT USERS
        [Authorize(Policy = "RequireUserRole")]
        [HttpPut(Name = "/UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] DtoUserInput user)
        {
            if (user == null)
            {
                return BadRequest("Invalid user input");
            }

            var userToUpdate = await _context.Users.FirstOrDefaultAsync(u => u.Account == user.Account);

            if (userToUpdate == null)
            {
                return Ok("Account does not exist");
            }

            var userbefore = new User
            {
                Account = userToUpdate.Account,
                Password = userToUpdate.Password,
                UserName = userToUpdate.UserName,
                LastLogin = userToUpdate.LastLogin,
                CreateAt = userToUpdate.CreateAt,
                UpdateAt = userToUpdate.UpdateAt,
            };

            // Cập nhật thông tin người dùng từ dữ liệu nhập vào
            userToUpdate.Password = user.Password;
            userToUpdate.UserName = user.UserName;
            userToUpdate.UpdateAt = DateTime.UtcNow;


            await _context.SaveChangesAsync();

            var response = new
            {
                UserBefore = userbefore,
                UserToUpdate = userToUpdate
            };

            return Ok(response);
        }




        //USER LOGIN
        [HttpPost(Name = "/UserLogin")]
        [RateLimitAttribute(3, 20)] //Lmit, WindowTime
        public async Task<IActionResult> UserLogin([FromBody] DtoLogin userlogin)
        {
            Log.Information("Received login request for account: {Account}", userlogin.Account);
            //Check null
            if (string.IsNullOrEmpty(userlogin.Account))
            {
                Log.Warning("Login request failed: Account is null or empty");
                return BadRequest("Please enter account");
            }

            if (string.IsNullOrEmpty(userlogin.Password))
            {
                Log.Warning("Login request failed: Password is null or empty");
                return BadRequest("Please enter password");
            }
            //Check account, password and create JWT 
            var AccountUser = await _context.Users.FirstOrDefaultAsync(b => b.Account == userlogin.Account);
            if (AccountUser == null)
            {
                Log.Warning("Login request failed: Account '{Account}' does not exist", userlogin.Account);
                return NotFound("Account doesn't exist");
            }
            if (AccountUser.Password != null)
            {
                if (!PasswordSecurity.VerifyPassword(userlogin.Password, AccountUser.Password))
                {
                    Log.Warning("Login request failed: Incorrect password for account '{Account}'", userlogin.Account);
                    return BadRequest("Incorrect password");
                }

                AccountUser.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                Log.Information("Received login request for account: {Account}", userlogin.Account);

                return Ok(new
                {
                    Message = "Login Success " + AccountUser.Account,
                    lastlogin = AccountUser.LastLogin,
                    Jwt = GenerateJwtToken(AccountUser)
                });
            }
            else { return BadRequest("Please enter password"); }
        }

        //HÀM TẠO JWT
        private string GenerateJwtToken(User username)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var securityKey = Encoding.UTF8.GetBytes(_appsetting.SecretKey ?? string.Empty);
            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", username.IdUser ?? string.Empty),
                    new Claim(ClaimTypes.Role, username.Role),
                    new Claim("Name", username.UserName ?? string.Empty),
                    new Claim("Account", username.Account ?? string.Empty),
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(securityKey), SecurityAlgorithms.HmacSha512Signature)
            };
            var token = jwtTokenHandler.CreateToken(tokenDescription);
            return jwtTokenHandler.WriteToken(token);
        }





        //Add Book To Favorites
        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost(Name = "/AddToFavorites")]
        public async Task<IActionResult> AddToFavorites(int bookId)
        {
            //Get Id string user from jwt token
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "Id");

            //Convert id string to 
            if (userIdClaim == null) { return NotFound(); }
            string userId = userIdClaim.Value;
            //Check if book exist
            var bookadd = await _context.Books.FindAsync(bookId);
            if (bookadd == null)
            {
                return BadRequest("Book doesn't exist");
            }
            //Check if book exist in FavoriteBooks
            var existingFavorite = await _context.FavoriteBooks.FirstOrDefaultAsync(fb => fb.UserId == userId && fb.BookId == bookId);
            if (existingFavorite != null)
            {
                return Conflict("This book is already in your favorites list.");
            }

            //Add to FavoriteBooks
            var favoriteBook = new FavoriteBook
            {
                UserId = userId,
                BookId = bookId,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            _context.FavoriteBooks.Add(favoriteBook);
            await _context.SaveChangesAsync();

            return Ok($"Book'{bookadd.Title}' added to favorites successfully.");
        }





        //Delete Favorites
        [Authorize(Policy = "RequireUserRole")]
        [HttpDelete(Name = "/DeleteFavorites")]
        public async Task<IActionResult> DeleteFavorites(int bookId)
        {
            //Get Id string user from jwt token and Convert id string to int
            var UserIdClaim = User.Claims.FirstOrDefault(c => c.Type == "Id");
            if (UserIdClaim == null) { return NotFound(); }
            int userId = int.Parse(UserIdClaim.Value);
            //Get Favorite Book delete and check if book exist
            var BookDelete = await _context.FavoriteBooks.FindAsync(userId, bookId);
            if (BookDelete == null)
            {
                return BadRequest("Book doesn't exist");
            }
            //Delete Favorite Book
            _context.FavoriteBooks.Remove(BookDelete);
            await _context.SaveChangesAsync();

            var book = await _context.Books.FindAsync(bookId);
            if (book == null) { return NotFound(); }
            return Ok($"Delete Favorite Book '{book.Title}' Success");
        }





        //Filter: Total likes of the book
        [HttpGet(Name = "/TotalLikeBook")]
        public async Task<IActionResult> TotalLikeBook(int bookId)
        {
            var Favorite = await _context.FavoriteBooks.CountAsync(i => i.BookId == bookId);
            if (Favorite == 0)
            {
                return BadRequest("Book doesn't exist or no one likes it yet");
            }
            return Ok($"Favorite number: {Favorite} Users");
        }





        //Filter: All UserNames like the book
        [HttpGet(Name = "/UserLikeBook")]
        public async Task<IActionResult> UserLikeBook(int bookId)
        {
            var userlike = await _context.FavoriteBooks
                .Where(fb => fb.BookId == bookId)
                .Select(fb => fb.UserId)
                .ToListAsync();
            var UserLikeBook = await _context.Users
                .Where(u => userlike.Contains(u.IdUser))
                .Select(u => new { u.IdUser, u.UserName }).ToListAsync();
            var namebook = await _context.Books.FindAsync(bookId);
            if (namebook == null) { return NotFound(); }
            return Ok(new
            {
                Massage = $"The book '{namebook.Title}' includes {UserLikeBook.Count} favorite users",
                UserLikeBook
            });
        }

    }
}
