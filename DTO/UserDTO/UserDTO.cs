using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIProgressTrackerApp.DTO.UserDTO
{
    public class LoginRequest
    {
        public string Email_address { get; set; }
        public string? Password { get; set; }
    }
    public class SignupRequest
    {
        public string Username { get; set; }
        public string Email_address { get; set; }
        public string Password { get; set; }
    }
    public class RefreshRequest
    {
        public string RefreshToken { get; set; }
    }
}