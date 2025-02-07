using generala.Models.Database.Entities;
using generala.Models.Database.Repositories;
using generala.Models.Database.Repositories.Implementations;
using TorchSharp.Modules;

namespace generala.Models.Database
{
    public class UnitOfWork
    {
        private readonly GeneralaContext _context;

        public UserRepository UserRepository { get; init; }

        public ImageRepository ImageRepository { get; init; }

        public FriendRequestRepository FriendRequestRepository { get; init; }

        public FriendshipRepository FriendshipRepository { get; init; }


        public UnitOfWork(
            GeneralaContext context,
            UserRepository userRepository,
            ImageRepository imageRepository,
            FriendRequestRepository friendRequestRepository, 
            FriendshipRepository friendshipRepository
            )
        {
            _context = context;
            UserRepository = userRepository;
            ImageRepository = imageRepository;
            FriendRequestRepository = friendRequestRepository;
            FriendshipRepository = friendshipRepository;
        }

        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
