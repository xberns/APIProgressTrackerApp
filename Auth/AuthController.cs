using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using APIProgressTrackerApp.DTO.UserDTO;
using apiprogresstracker.Model.User;
using apiprogresstracker.ApplicationDBContext;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly TokenService _tokenService;
    private readonly EmailService _emailService;

    public AuthController(
        ApplicationDbContext context,
        TokenService tokenService,
        EmailService emailService)
    {
        _context = context;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email_address) ||
                string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("All fields are required.");

            // Username validation
            if (request.Username.Length < 3 || request.Username.Length > 20)
                return BadRequest("Username must be between 3 and 20 characters.");
            if (!Regex.IsMatch(request.Username, @"^[a-zA-Z0-9_.]+$"))
                return BadRequest("Username can only contain letters, numbers, underscores, or dots.");

            // Email validation
            try { var addr = new System.Net.Mail.MailAddress(request.Email_address); }
            catch { return BadRequest("Invalid email format."); }

            var allowedDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "gmail.com", "outlook.com", "yahoo.com"
            };
            var emailDomain = request.Email_address.Split('@').Last();
            if (!allowedDomains.Contains(emailDomain))
                return BadRequest("Email domain not supported.");

            // Password validation
            var password = request.Password;
            if (password.Length < 8 || !password.Any(char.IsUpper) || !password.Any(char.IsLower) ||
                !password.Any(char.IsDigit) || !password.Any(ch => "!@#$%^&*()_-+=<>?/{}~|".Contains(ch)))
                return BadRequest("Password must meet complexity requirements.");

            if (await _context.UserAccount.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Username already exists.");
            if (await _context.UserAccount.AnyAsync(u => u.Email_address == request.Email_address))
                return BadRequest("Email already exists.");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
            var verificationToken = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid().ToString();

            var user = new UserAccount
            {
                Username = request.Username,
                Email_address = request.Email_address,
                User_id = userId,
                Date_created = DateTime.UtcNow,
                PasswordHash = passwordHash,
                Verification_token = verificationToken,
                Is_verified = 0,
                Is_active = 0
            };

            _context.UserAccount.Add(user);
            var saved = await _context.SaveChangesAsync();
            if (saved <= 0) return StatusCode(500, "Account creation failed.");

            _ = Task.Run(async () =>
            {
                try { await _emailService.SendVerificationEmail(user.Email_address, user.Username, verificationToken); }
                catch (Exception ex) { Console.WriteLine("Email failed: " + ex.Message); }
            });

            return Ok("User created successfully! Please check your email to verify your account.");
        }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }

    [HttpGet("verify")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        var user = await _context.UserAccount.FirstOrDefaultAsync(u => u.Verification_token == token);
        if (user == null) return BadRequest("Invalid verification token");

        user.Is_verified = 1;
        user.Is_active = 1;
        user.Verification_token = null;
        await _context.SaveChangesAsync();

        return Ok("Email verified successfully! You can now log in.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email_address) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and password are required.");

            var user = await _context.UserAccount
                .FirstOrDefaultAsync(u => u.Email_address == request.Email_address);

            if (user == null) return Unauthorized("Invalid username or password");
            if (user.Is_verified == 0) return Unauthorized("Email not verified.");

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid) return Unauthorized("Invalid username or password");

            var accessToken = _tokenService.GenerateToken(user.User_id);
            var refreshTokenValue = _tokenService.GenerateRefreshToken();
            var refreshToken = new RefreshToken
            {
                Token = refreshTokenValue,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                Date_created = DateTime.UtcNow,
                IsRevoked = 0,
                User_id = user.User_id,
            };

            _context.RefreshToken.Add(refreshToken);
            await _context.SaveChangesAsync();
            //Match cookies
             Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
            Response.Cookies.Append("refreshToken", refreshTokenValue, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            });
            return Ok(new
            {
                accessToken,
                username = user.Username
            });
        }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }
    //[Authorize]
    [HttpGet("verify-token")]
    public IActionResult VerifyToken()
    {
        return Ok(new { message = "Token is valid" });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        
        var refreshTokenValue = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshTokenValue))
            return Unauthorized("No refresh token found");


        var refreshToken = await _context.RefreshToken
            .Include(rt => rt.UserAccount)
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenValue);

        if (refreshToken == null) {  
            Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });

            return Unauthorized("Invalid refresh token");
        }

        if (refreshToken.ExpiryDate < DateTime.UtcNow) {
              Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });

            return Unauthorized("Refresh token expired");
        }

        if (refreshToken.IsRevoked == 1) {
              Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });

            return Unauthorized("Refresh token revoked");
        }

        var user = refreshToken.UserAccount;

        refreshToken.IsRevoked = 1;
        refreshToken.Date_used = DateTime.UtcNow;

        var newRefreshTokenValue = _tokenService.GenerateRefreshToken();
        var newRefreshToken = new RefreshToken
        {
            Token = newRefreshTokenValue,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            User_id = user.User_id
        };
        _context.RefreshToken.Add(newRefreshToken);

        var newAccessToken = _tokenService.GenerateToken(user.User_id);

        await _context.SaveChangesAsync();

         Response.Cookies.Append("refreshToken", refreshTokenValue, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            });

        return Ok(new
        {
            accessToken = newAccessToken
        });
    }
    
   [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshTokenValue = Request.Cookies["refreshToken"];

        if (!string.IsNullOrEmpty(refreshTokenValue))
        {
            var token = await _context.RefreshToken
                .FirstOrDefaultAsync(t => t.Token == refreshTokenValue);

            if (token != null)
            {
                token.IsRevoked = 1;
                token.Date_used = DateTime.UtcNow;
                _context.RefreshToken.Update(token);
                await _context.SaveChangesAsync();
            }
        }

        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });

        return Ok(new { message = "Logged out successfully" });
    }

}
