namespace ChatAppSignalR.DTOs
{
    public class SenderInfoDto
    {
        public string Id { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? AvatarUrl { get; set; }
    }
}