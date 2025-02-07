using generala.Models.Database;
using generala.Models.Database.Entities;
using generala.Models.Database.Repositories.Implementations;
using generala.Models.Mappers;
using generala.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace generala.Controllers
{
    [ApiController]
    [Route("api/friend-requests")]
    public class FriendRequestController : ControllerBase
    {
        private readonly FriendRequestRepository _friendRequestRepository;
        private readonly FriendshipRepository _friendshipRepository;
        private readonly GeneralaContext _context;

        public FriendRequestController(FriendRequestRepository friendRequestRepository, FriendshipRepository friendshipRepository, GeneralaContext context)
        {
            _friendRequestRepository = friendRequestRepository;
            _friendshipRepository = friendshipRepository;
            _context = context;
        }

        // 📌 Enviar solicitud de amistad
        [HttpPost("send")]
        public async Task<IActionResult> SendFriendRequest(int senderId, int receiverId)
        {
            var existingRequest = await _friendRequestRepository.GetRequestAsync(senderId, receiverId);
            var alreadyFriends = await _friendshipRepository.AreFriendsAsync(senderId, receiverId);

            if (existingRequest != null || alreadyFriends)
            {
                return BadRequest("La solicitud ya fue enviada o ya son amigos.");
            }

            var request = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                IsAccepted = false
            };

            await _friendRequestRepository.InsertAsync(request);
            await _context.SaveChangesAsync();

            return Ok(FriendRequestMapper.ToDto(request));
        }

        // 📌 Aceptar solicitud de amistad
        [HttpPost("accept")]
        public async Task<IActionResult> AcceptFriendRequest(int requestId)
        {
            var request = await _friendRequestRepository.GetByIdAsync(requestId);
            if (request == null) return NotFound("Solicitud no encontrada.");

            request.IsAccepted = true;

            var friendship = new Friendship
            {
                User1Id = request.SenderId,
                User2Id = request.ReceiverId
            };

            await _friendshipRepository.InsertAsync(friendship);
            _friendRequestRepository.Delete(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Solicitud aceptada, ahora son amigos." });
        }

        // 📌 Rechazar solicitud de amistad
        [HttpDelete("reject/{requestId}")]
        public async Task<IActionResult> RejectFriendRequest(int requestId)
        {
            var request = await _friendRequestRepository.GetByIdAsync(requestId);
            if (request == null) return NotFound("Solicitud no encontrada.");

            _friendRequestRepository.Delete(request);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Solicitud rechazada." });
        }

        // 📌 Obtener solicitudes pendientes
        [HttpGet("pending/{userId}")]
        public async Task<IActionResult> GetPendingRequests(int userId)
        {
            var requests = await _friendRequestRepository.GetReceivedRequestsAsync(userId);
            var requestDtos = requests.Select(FriendRequestMapper.ToDto).ToList();
            return Ok(requestDtos);
        }

        [HttpGet("sent/{userId}")]
        public async Task<IActionResult> GetSentRequests(int userId)
        {
            var sentRequests = await _context.Friend_request
                .Where(fr => fr.SenderId == userId && !fr.IsAccepted)
                .ToListAsync();

            var requestDtos = sentRequests.Select(FriendRequestMapper.ToDto).ToList();
            return Ok(requestDtos);
        }

    }
}
