using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace apiprogresstracker.Model.User
{
    public class UserAccount
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email_address { get; set; }
        public string? PasswordHash { get; set; }
        public string? User_id { get; set; }
        public DateTime? Date_created { get; set; }
        public int? Is_verified { get; set; }
        public int? Is_active { get; set; }
        public string? Verification_token { get; set; }
        public List<RefreshToken>? RefreshTokens { get; set; }
    }
    public class RefreshToken
    {
        public int Id { get; set; }

        public string Token { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int IsRevoked { get; set; }

        // foreign key
        public string? User_id { get; set; }
        public UserAccount UserAccount { get; set; }
    }


}