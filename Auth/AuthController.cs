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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email_address) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email and password are required.");
            }   
            var user = await _context.UserAccount
                .FirstOrDefaultAsync(u => u.Email_address == request.Email_address);

            if (user == null)
                return Unauthorized("Invalid username or password");

            if (user.Is_verified == 0)
                return Unauthorized("Email not verified. Please check your inbox.");

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
                return Unauthorized("Invalid username or password");

            var token = _tokenService.GenerateToken(user.User_id.ToString());

            return Ok(new { accessToken = token });
        }
        catch(Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
        try
        {
            
           if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email_address) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("All fields are required.");
            }
            if (request.Username.Length < 3 || request.Username.Length > 20)
                 return BadRequest("Username must be between 3 and 20 characters.");
            var usernameRegex = @"^[a-zA-Z0-9_.]+$";
            if (!Regex.IsMatch( request.Username, usernameRegex))
                return BadRequest("Username can only contain letters, numbers, underscores, or dots.");

            try
            {
                var addr = new System.Net.Mail.MailAddress(request.Email_address);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid email format.");
            }

            var allowedDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "gmail.com",
                "outlook.com",
                "yahoo.com",
            };

            var emailDomain = request.Email_address.Split('@').Last();
            if (!allowedDomains.Contains(emailDomain))
            {
                return BadRequest("Email domain not supported. Use Gmail, Outlook, or Yahoo.");
            }

            var password = request.Password;

            if (password.Length < 8)
                return BadRequest("Password must be at least 8 characters long.");

            if (!password.Any(char.IsUpper))
                return BadRequest("Password must contain at least one uppercase letter.");

            if (!password.Any(char.IsLower))
                return BadRequest("Password must contain at least one lowercase letter.");

            if (!password.Any(char.IsDigit))
                return BadRequest("Password must contain at least one number.");

            if (!password.Any(ch => "!@#$%^&*()_-+=<>?/{}~|".Contains(ch)))
                return BadRequest("Password must contain at least one special character.");
            
            if (await _context.UserAccount.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Username already exists.");

            if (await _context.UserAccount.AnyAsync(u => u.Email_address == request.Email_address))
                return BadRequest("Email already exists.");

           var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);
           
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
    
                if (saved <= 0)
                {
                    return StatusCode(500, "Account creation failed.");
                }
        _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendVerificationEmail(user.Email_address, user.Username, verificationToken);
                    Console.WriteLine("Verification email sent successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Email failed: " + ex.Message);
                }
            }
            );
            return Ok("User created successfully! Please check your email to verify your account.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("verify")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        var user = await _context.UserAccount.FirstOrDefaultAsync(u => u.Verification_token == token);
        if (user == null)
            return BadRequest("Invalid verification token");

        user.Is_verified = 1;
        user.Is_active =1;
        user.Verification_token = null;
        await _context.SaveChangesAsync();

        return Ok("Email verified successfully! You can now log in.");
    }

}
