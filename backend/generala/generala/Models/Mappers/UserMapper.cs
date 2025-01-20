using generala.Models.Database.Entities;
using generala.Models.Dtos;


namespace generala.Models.Mappers;

public class UserMapper
{
    public UserDto UserToDto(User user)
    {
        return new UserDto
        {
            UserId = user.Id,
            Nickname = user.Nickname,
            Email = user.Email,
            Is_banned = user.Is_banned,
            Role = user.Role,
            Image = user.Image
        };
    }

    public UserProfileDto UserProfileToDto(User user)
    {
        return new UserProfileDto
        {
            UserId = user.Id,
            Nickname = user.Nickname,
            Email = user.Email,
            Is_banned = user.Is_banned,
            Role = user.Role,
            Password = user.Password,
            Image = user.Image
        };
    }

    public IEnumerable<UserDto> UsersToDto(IEnumerable<User> users)
    {
        return users.Select(UserToDto);
    }

}
