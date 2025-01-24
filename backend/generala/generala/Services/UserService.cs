using generala.Controllers;
using generala.Helpers;
using generala.Models.Database;
using generala.Models.Database.Entities;
using generala.Models.Database.Repositories.Implementations;
using generala.Models.Dtos;
using generala.Models.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace generala.Services;

public class UserService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly UserMapper _userMapper;

    public UserService(UnitOfWork unitOfWork, UserMapper userMapper)
    {
        _unitOfWork = unitOfWork;
        _userMapper = userMapper;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.UserRepository.GetAllAsync();
        return _userMapper.UsersToDto(users).ToList();
    }

    public async Task<UserDto> GetUserByEmailAsync(string email)
    {
        var user = await _unitOfWork.UserRepository.GetUserByEmail(email);
        if (user == null)
        {
            return null;
        }
        return _userMapper.UserToDto(user);
    }

    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var user = await _unitOfWork.UserRepository.GetUserById(id);

        if (user == null)
        {
            return null;
        }

        return _userMapper.UserToDto(user);
    }

    public async Task<User> GetUserByIdAsyncNoDto(int id)
    {
        var user = await _unitOfWork.UserRepository.GetUserById(id);

        if (user == null)
        {
            return null;
        }

        return user;
    }

    public async Task<User> LoginAsync(string identifier, string password)
    {
        var user = await _unitOfWork.UserRepository.GetUserByIdentifierAsync(identifier);

        if (user == null || user.Password != PasswordHelper.Hash(password))
        {
            return null;
        }

        return user;
    }

    public async Task<User> RegisterAsync(RegisterDto model)
    {
        // Verifica si ya existe un usuario con el mismo correo
        var existingUserByEmail = await _unitOfWork.UserRepository.GetUserByEmail(model.Email);
        if (existingUserByEmail != null)
        {
            throw new Exception("El correo electrónico ya está en uso.");
        }

        // Verifica si ya existe un usuario con el mismo nickname
        var existingUserByNickname = await _unitOfWork.UserRepository.GetQueryable()
            .FirstOrDefaultAsync(u => u.Nickname == model.Nickname);

        if (existingUserByNickname != null)
        {
            throw new Exception("El nickname ya está en uso.");
        }

        // Guarda la imagen del avatar si se proporciona
        string imageUrl = await SaveAvatarAsync(model.Image);

        var newUser = new User
        {
            Email = model.Email,
            Nickname = model.Nickname,
            Is_banned = false,
            Role = "User", // Rol por defecto
            Password = PasswordHelper.Hash(model.Password),
            Image = imageUrl
        };

        await _unitOfWork.UserRepository.InsertUserAsync(newUser);
        await _unitOfWork.SaveAsync();

        return newUser;
    }
    // Modificar los datos del usuario
    public async Task ModifyUserAsync(UserProfileDto userDto)
    {
        var existingUser = await _unitOfWork.UserRepository.GetUserById(userDto.UserId);

        if (existingUser == null)
        {
            throw new Exception($"El usuario con ID {userDto.UserId} no existe.");
        }

        if (!string.IsNullOrEmpty(userDto.Nickname))
        {
            existingUser.Nickname = userDto.Nickname;
        }

        if (!string.IsNullOrEmpty(userDto.Email))
        {
            existingUser.Email = userDto.Email;
        }

        if (!string.IsNullOrEmpty(userDto.Password))
        {
            existingUser.Password = PasswordHelper.Hash(userDto.Password);
        }

        

        await UpdateUser(existingUser);
        await _unitOfWork.SaveAsync();
    }
    // Modificar el rol del usuario
    public async Task ModifyUserRoleAsync(int userId, string newRole)
    {
        var existingUser = await _unitOfWork.UserRepository.GetUserById(userId);

        if (existingUser == null)
        {
            throw new Exception($"El usuario con ID {userId} no existe.");
        }

        if (!string.IsNullOrEmpty(newRole))
        {
            existingUser.Role = newRole;
        }

        await UpdateUser(existingUser);
        await _unitOfWork.SaveAsync();
    }
    // Eliminar usuario
    public async Task DeleteUserAsync(int userId)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

        if (user == null)
        {
            throw new InvalidOperationException("El usuario no existe.");
        }

        _unitOfWork.UserRepository.DeleteUser(user);
        await _unitOfWork.SaveAsync();
    }

    public async Task UpdateUser(User user)
    {
        _unitOfWork.UserRepository.Update(user);
        await _unitOfWork.SaveAsync();
    }

    private async Task<string> SaveAvatarAsync(IFormFile avatar)
    {
        if (avatar == null)
        {
            return null; // URL predeterminada para avatares
        }

        var folderPath = Path.Combine("wwwroot", "avatars");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{avatar.FileName}";
        var filePath = Path.Combine(folderPath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await avatar.CopyToAsync(stream);
        }

        return $"/avatars/{uniqueFileName}";
    }

    public async Task<User> GetUserByNicknameAsync(string nickname)
    {
        return await _unitOfWork.UserRepository.GetUserByNicknameAsync(nickname);
    }
}
