using generala.Helpers;
using generala.Models.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace generala.Models.Database
{
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
            await SeedFriendshipsAsync();

            await _context.SaveChangesAsync();
        }

        private async Task SeedUsersAsync()
        {
            if (!_context.User.Any()) 
            {
                User[] users = [
                    new User {
                        Nickname = "Agustin",
                        Email = "agustin@gmail.com",
                        Password = PasswordHelper.Hash("123456"),
                        Is_banned = false,
                        Role = "Admin",
                        Image = "avatars/imagen-avatar.png"
                    },
                    new User {
                        Nickname = "Antonio",
                        Email = "antonio@gmail.com",
                        Password = PasswordHelper.Hash("123456"),
                        Is_banned = false,
                        Role = "User",
                        Image = "avatars/imagen-avatar.png"
                    }
                ];

                await _context.User.AddRangeAsync(users);
                await _context.SaveChangesAsync(); 
            }
        }

        private async Task SeedFriendshipsAsync()
        {
            
            var user1 = await _context.User.FirstOrDefaultAsync(u => u.Email == "agustin@gmail.com");
            var user2 = await _context.User.FirstOrDefaultAsync(u => u.Email == "antonio@gmail.com");

            if (user1 != null && user2 != null)
            {
                
                if (!_context.Friendship.Any(f => (f.User1Id == user1.Id && f.User2Id == user2.Id) ||
                                                   (f.User1Id == user2.Id && f.User2Id == user1.Id)))
                {
                    Friendship friendship = new Friendship
                    {
                        User1Id = user1.Id,
                        User2Id = user2.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.Friendship.AddAsync(friendship);
                }
            }
        }
    }
}
