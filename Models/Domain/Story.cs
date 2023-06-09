﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models.Domain
{
    public class Story
    {
        public int StoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18, 3)")]
        public decimal OneTimePrice { get; set; }
        [Column(TypeName = "decimal(18, 3)")]
        public decimal LifeTimePrice { get; set; }
        public string Description { get; set; } = string.Empty;
        public string AvatarImage { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        // Danh sách user mua story tương ứng
        public List<AppUser> Users { get; } = new();
        // Danh sách các Order đặt Story tương ứng
        public List<Order> Orders { get; } = new();
    }
}
