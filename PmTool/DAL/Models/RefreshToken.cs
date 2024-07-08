using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class RefreshToken : BaseEntity
    {
        public int RefreshTokenId { get; set; }
        public int UserId { get; set; } // Linked to the AspNet Identity User Id
        public User User { get; set; }
        public string Token { get; set; }
        public string JwtId { get; set; } // Map the token with jwtId
        public bool IsUsed { get; set; } // if its used we dont want generate a new Jwt token with the same refresh token
        public bool IsRevoked { get; set; } // if it has been revoke for security reasons
        public DateTime ExpiryDate { get; set; } // Refresh token is long lived it could last for months.

    }
}
