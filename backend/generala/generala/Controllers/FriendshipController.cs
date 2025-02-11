using generala.Models.Database;
using generala.Models.Database.Entities;
using generala.Models.Database.Repositories.Implementations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace generala.Controllers
{
    [ApiController]
    [Route("api/friendships")]
    public class FriendshipController : ControllerBase
    {
        private readonly FriendshipRepository _friendshipRepository;
        private readonly GeneralaContext _context;

        public FriendshipController(FriendshipRepository friendshipRepository, GeneralaContext context)
        {
            _friendshipRepository = friendshipRepository;
            _context = context;
        }

        //Obtener la lista de amigos de un usuario
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetFriends(int userId)
        {
            var friends = await _friendshipRepository.GetFriendsAsync(userId);
            return Ok(friends);
        }

        //Eliminar un amigo
        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveFriend(int userId, int friendId)
        {
            var friendship = await _friendshipRepository.GetQueryable()
                .FirstOrDefaultAsync(f => (f.User1Id == userId && f.User2Id == friendId) ||
                                          (f.User1Id == friendId && f.User2Id == userId));

            if (friendship == null)
            {
                return NotFound("No se encontró la relación de amistad.");
            }

            _friendshipRepository.Delete(friendship);
            await _context.SaveChangesAsync();
            return Ok("Amistad eliminada.");
        }
    }
}
