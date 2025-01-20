namespace generala.Models.Dtos
{
    public class UserProfileDto
    {
        public int UserId { get; set; }

        public string Nickname { get; set; } = null!;

        public string Email { get; set; } = null!;

        public Boolean Is_banned { get; set; } = false;

        public string Role { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string Image { get; set; } = null!;
    }
}
