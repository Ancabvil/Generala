using generala.Models.Database.Repositories;
using generala.Models.Database.Repositories.Implementations;
using generala.Models.Database.Repositories.Implementations;
using TorchSharp.Modules;

namespace generala.Models.Database
{
    public class UnitOfWork
    {
        private readonly GeneralaContext _context;

        public UserRepository UserRepository { get; init; }


        public UnitOfWork(
            GeneralaContext context,
            UserRepository userRepository
            )
        {
            _context = context;
            UserRepository = userRepository;
        }

        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
