
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class User : BaseEntity
    {
        public int UserId { get; set; }
        public Guid UserPublicId { get; set; }

        [MaxLength(60)]
        public string FullName { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        public string Password { get; set; }

        public int? AccessFailedCount { get; set; }

        public bool? IsLocked { get; set; }

        public DateTime? LockedDate { get; set; }

        [MaxLength(100)]
        public string MobileNumber { get; set; }

        public bool? IsVerified { get; set; }

        public int? RetryDurationInMinutes { get; set; }

        public string PictureURL { get; set; }

        public bool? IsProfileCompleted { get; set; }

        public DateTime? VerificationDate { get; set; }
        [MaxLength(15)]
        public string IPAddress { get; set; }

        public DateTime? LastLoggedInDate { get; set; }

        public bool? IsReset { get; set; }

        [MaxLength(10)]
        public string ResetCode { get; set; }

        public int? RoleId { get; set; }
        public Role Role { get; set; }

        public bool IsSocialUser { get; set; }
        public string Provider { get; set; }
        public int? Gender { get; set; } //1 For MALE, 2 For FEMALE

    }
}
