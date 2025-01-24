﻿namespace generala.Models.Dtos
{
    public class RegisterDto
    {
        public string Nickname { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;

        public IFormFile Image { get; set; }

    }

}
