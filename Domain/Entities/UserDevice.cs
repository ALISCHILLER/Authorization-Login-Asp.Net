using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization_Login_Asp.Net.Domain.Entities
{
    public class UserDevice
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        [MaxLength(100)]
        public string DeviceId { get; set; }

        [MaxLength(200)]
        public string DeviceName { get; set; }

        [MaxLength(50)]
        public string DeviceType { get; set; }

        [MaxLength(50)]
        public string OperatingSystem { get; set; }

        [MaxLength(200)]
        public string UserAgent { get; set; }

        public DateTime LastUsedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [MaxLength(50)]
        public string IpAddress { get; set; }

        [MaxLength(100)]
        public string Location { get; set; }
    }
} 