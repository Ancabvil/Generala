﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace generala.Models.Database.Entities
{
    [Index(nameof(Id), IsUnique = true)]
    public class Friendship
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("User1")]
        public int User1Id { get; set; }
        public User User1 { get; set; } = null!;

        [ForeignKey("User2")]
        public int User2Id { get; set; }
        public User User2 { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
