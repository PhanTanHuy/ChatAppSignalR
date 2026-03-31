namespace ChatAppSignalR.DTOs
{
    public class ConversationResponse
    {
        public string Id { get; set; } = null!;
        public bool IsDirect { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public string? LastMessageId { get; set; }
        public List<UserDto> Participants { get; set; } = new List<UserDto>();
        public List<string> SeenBy { get; set; } = new List<string>();
        public Dictionary<string, int> UnreadCounts { get; set; } = new Dictionary<string, int>();
        public DateTime CreatedAt { get; set; }
        public LastMessageResponse? LastMessage { get; set; }
    }

    public class LastMessageResponse
    {
        public string Id { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public SenderInfoDto Sender { get; set; } = null!;
    }
}
