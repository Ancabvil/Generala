using generala.Models.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace generala.Models.Database.Repositories.Implementations
{
    public class FriendshipRepository : Repository<Friendship, int>
    {
        public FriendshipRepository(GeneralaContext context) : base(context) { }

        //lista de amigos
        public async Task<List<User>> GetFriendsAsync(int userId)
        {
            return await GetQueryable()
                .Where(f => f.User1Id == userId || f.User2Id == userId)
                .Select(f => f.User1Id == userId ? f.User2 : f.User1)
                .ToListAsync();
        }

        //Verificar si dos usuarios son amigos
        public async Task<bool> AreFriendsAsync(int user1Id, int user2Id)
        {
            return await GetQueryable()
                .AnyAsync(f => (f.User1Id == user1Id && f.User2Id == user2Id) ||
                               (f.User1Id == user2Id && f.User2Id == user1Id));
        }
    }
}
