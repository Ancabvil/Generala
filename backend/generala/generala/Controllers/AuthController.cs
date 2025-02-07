using generala.Helpers;
using generala.Models.Database;
using generala.Models.Database.Entities;
using generala.Models.Dtos;
using generala.Models.Mappers;
using generala.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace generala.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly TokenValidationParameters _tokenParameters;
    private readonly UserService _userService;
    private readonly UserMapper _userMapper;
    private readonly WebSocketNotificationService _webSocketNotificationService;

    public AuthController(UserService userService, UserMapper userMapper, IOptionsMonitor<JwtBearerOptions> jwtOptions, WebSocketNotificationService webSocketNotificationService)
    {
        _userService = userService;
        _userMapper = userMapper;
        _tokenParameters = jwtOptions.Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters;
        _webSocketNotificationService = webSocketNotificationService;
    }

    // LOGIN 
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResult>> Login([FromBody] LoginRequest model)
    {
        try
        {
            var user = await _userService.LoginAsync(model.Identifier, model.Password);

            if (user == null)
            {
                return Unauthorized("Datos de inicio de sesión incorrectos.");
            }

            var claims = new List<Claim>
{
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("nickname", user.Nickname)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = new Dictionary<string, object>
            {
                { ClaimTypes.NameIdentifier, user.Id },
                { ClaimTypes.Role, user.Role }
                
            },
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddYears(5),
                SigningCredentials = new SigningCredentials(
                    _tokenParameters.IssuerSigningKey,
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            string stringToken = tokenHandler.WriteToken(token);

            var loginResult = new LoginResult
            {
                AccessToken = stringToken,
                User = _userMapper.UserToDto(user)
            };

            return Ok(loginResult);
        }
        catch (InvalidOperationException)
        {
            return Unauthorized("Datos de inicio de sesión incorrectos.");
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized("Usuario no autenticado.");
        }

        int userId = int.Parse(userIdClaim.Value);
        _webSocketNotificationService.RemoveConnection(userId);

        await _webSocketNotificationService.BroadcastMessageAsync(new WebSocketMessageDto
        {
            Type = "user_disconnected",
            Content = JsonSerializer.Serialize(new { userId })
        });

        return Ok(new { message = "Sesión cerrada correctamente.", userId });
    }



    // SIGN UP CREAR NUEVO USUARIO
    [HttpPost("Signup")]
    public async Task<ActionResult<RegisterDto>> SignUp([FromForm] RegisterDto model)
    {
        //Verifica si el modelo recibido es válido
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        //Verifica si ya existe un usuario con el mismo correo
        var existingUserByEmail = await _userService.GetUserByEmailAsync(model.Email);
        if (existingUserByEmail != null)
        {
            return Conflict(new { message = "El correo electrónico ya está en uso." });
        }

        //Verifica si ya existe un usuario con el mismo nickname
        var existingUserByNickname = await _userService.GetUserByNicknameAsync(model.Nickname);
        if (existingUserByNickname != null)
        {
            return Conflict(new { message = "El nickname ya está en uso." });
        }

        try
        {
            //Crea un nuevo usuario
            var newUser = await _userService.RegisterAsync(model);

            //Mapea el nuevo usuario al DTO
            var userDto = _userMapper.UserToDto(newUser);


            return CreatedAtAction(nameof(Login), new { email = userDto.Email }, userDto);
        }
        catch (Exception ex)
        {
            //errores generales
            return StatusCode(500, new { message = "Ocurrió un error inesperado.", detail = ex.Message });
        }
    }
}
