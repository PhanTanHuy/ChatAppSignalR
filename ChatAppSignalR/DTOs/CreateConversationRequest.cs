namespace ChatAppSignalR.DTOs
{
    public class CreateConversationRequest
    {
        public bool IsDirect { get; set; } = true;
        public List<string> ParticipantIds { get; set; } = new List<string>();
    }
}
