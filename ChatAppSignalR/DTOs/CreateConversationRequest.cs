namespace ChatAppSignalR.DTOs
{
    public class CreateConversationRequest
    {
        public bool IsDirect { get; set; } = false;
        public string? GroupName { get; set; }
        public List<string> ParticipantIds { get; set; } = new List<string>();
    }
}
