using generala.Models.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace generala.Models.Database.Repositories.Implementations
{
    public class FriendRequestRepository : Repository<FriendRequest, int>
    {
        public FriendRequestRepository(GeneralaContext context) : base(context) { }

        //solicitudes de amistad enviadas por un usuario
        public async Task<List<FriendRequest>> GetSentRequestsAsync(int senderId)
        {
            return await GetQueryable()
                .Where(fr => fr.SenderId == senderId)
                .ToListAsync();
        }

        //olicitudes de amistad recibidas para un usuario
        public async Task<List<FriendRequest>> GetReceivedRequestsAsync(int receiverId)
        {
            return await GetQueryable()
                .Where(fr => fr.ReceiverId == receiverId && !fr.IsAccepted)
                .Include(fr => fr.Sender)  
                .Include(fr => fr.Receiver) 
                .ToListAsync();
        }


        //solicitud especifica
        public async Task<FriendRequest> GetRequestAsync(int senderId, int receiverId)
        {
            return await GetQueryable()
                .FirstOrDefaultAsync(fr => fr.SenderId == senderId && fr.ReceiverId == receiverId);
        }
    }
}
