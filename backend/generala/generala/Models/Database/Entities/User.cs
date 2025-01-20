using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace generala.Models.Database.Entities;

[Index(nameof(Email), IsUnique = true)]
[Index(nameof(Id), IsUnique = true)]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Nickname { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public Boolean Is_banned { get; set; } = false;

    public string Role { get; set; } = null!;

    public string Image { get; set; }

}
