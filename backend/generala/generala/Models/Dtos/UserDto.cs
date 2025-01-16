namespace generala.Models.Dtos
{
    public class UserDto
    {
        public int UserId { get; set; }

        public string Nickname { get; set; } = null!;

        public string Email { get; set; } = null!;

        public Boolean Is_banned { get; set; } = false;

        public string Role { get; set; } = null!;

    }
}
