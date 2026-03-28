using System.ComponentModel.DataAnnotations;

namespace ChatAppSignalR.DTOs
{
    public class UserSearchQuery
    {
        [Required(ErrorMessage = "Username là bắt buộc")]
        [MinLength(2, ErrorMessage = "Username phải có ít nhất 2 ký tự")]
        public string Username { get; set; } = null!;
    }
}