namespace generala.Models.Dtos
{
    public class LoginRequest
    {
        public string Identifier { get; set; } = null!; //nickname o email
        public string Password { get; set; } = null!;

    }
}
