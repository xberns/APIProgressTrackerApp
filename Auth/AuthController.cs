using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var user = await _context.UserAccount
                .FirstOrDefaultAsync(u => u.Username == request.Username);

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
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (await _context.UserAccount.AnyAsync(u => u.Username == request.Username))
            return BadRequest("Username already exists");

            if (await _context.UserAccount.AnyAsync(u => u.Email_address == request.Email_address))
                return BadRequest("Email already exists");

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

            var success = await _emailService.SendVerificationEmail(user.Email_address,user.Username, verificationToken);
            if (saved == 0 || success == false)
                {
                    await transaction.RollbackAsync();
                }
            await transaction.CommitAsync();
            return Ok("User created successfully! Please check your email to verify your account.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
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
