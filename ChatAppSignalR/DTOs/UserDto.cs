namespace ChatAppSignalR.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public bool IsFriend { get; set; }
        public string Email {get; set; } = null!;
        public string? DisplayName { get; set; }
    }
}
