namespace ChatAppSignalR.DTOs
{
    public class MessageResponse
    {
        public string Id { get; set; } = null!;
        public string ConversationId { get; set; } = null!;
        public string SenderId { get; set; } = null!;
        public string? Content { get; set; }
        public string? ImgUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public SenderInfoDto? Sender { get; set; }
    }
}