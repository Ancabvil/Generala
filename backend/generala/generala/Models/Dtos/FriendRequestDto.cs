namespace generala.Models.Dtos
{
    public class FriendRequestDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderNickname { get; set; } = null!;
        public int ReceiverId { get; set; }
        public string ReceiverNickname { get; set; } = null!;
        public bool IsAccepted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
