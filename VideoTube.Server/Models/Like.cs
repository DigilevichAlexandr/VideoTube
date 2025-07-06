using System;
using System.ComponentModel.DataAnnotations;

namespace VideoTube.Server.Models
{
    public class Like
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string VideoFileName { get; set; } = string.Empty;

        [Required]
        public string IP { get; set; } = string.Empty;

        // Можно добавить UserId, IP, или fingerprint для уникальности, но сейчас анонимно
        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }
} 