using generala.Helpers;
using generala.Models.Database.Entities;


namespace generala.Models.Database;

public class Seeder
{
    private readonly GeneralaContext _context;

    public Seeder(GeneralaContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await SeedUsersAsync();
        //await SeedFriendshipAsync();

        await _context.SaveChangesAsync();
    }

    private async Task SeedUsersAsync()
    {
        User[] users = [
                new User {
                    Nickname = "Agustin" ,
                    Email = "agustin@gmail.com",
                    Password = PasswordHelper.Hash("123456"),
                    Is_banned = false,
                    Role = "Admin"
                }
                ,
                new User {
                    Nickname = "Antonio" ,
                    Email = "antonio@gmail.com",
                    Password = PasswordHelper.Hash("123456"),
                    Is_banned = false,
                    Role = "User"
                }
            ];

        await _context.User.AddRangeAsync(users);
    }
}