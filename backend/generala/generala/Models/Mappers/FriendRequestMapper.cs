using generala.Models.Database.Entities;
using generala.Models.Dtos;

namespace generala.Models.Mappers
{
    public class FriendRequestMapper
    {
        public static FriendRequestDto ToDto(FriendRequest request)
        {
            return new FriendRequestDto
            {
                Id = request.Id,
                SenderId = request.SenderId,
                SenderNickname = request.Sender?.Nickname ?? "Desconocido", 
                ReceiverId = request.ReceiverId,
                ReceiverNickname = request.Receiver?.Nickname ?? "Desconocido",
                IsAccepted = request.IsAccepted,
                CreatedAt = request.CreatedAt
            };
        }

        public static FriendRequest ToEntity(FriendRequestDto dto)
        {
            return new FriendRequest
            {
                Id = dto.Id,
                SenderId = dto.SenderId,
                ReceiverId = dto.ReceiverId,
                IsAccepted = dto.IsAccepted,
                CreatedAt = dto.CreatedAt
            };
        }
    }
}
