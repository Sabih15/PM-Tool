using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class UserResetPassword : BaseEntity
    {
        public int UserResetPasswordId { get; set; }
        public string Email { get; set; }
        public string VerificationKey { get; set; }
        
    }
}
