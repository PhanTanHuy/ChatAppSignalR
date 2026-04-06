using System.ComponentModel.DataAnnotations;

namespace ChatAppSignalR.DTOs
{
    public class AddMemberRequest
    {
        [Required(ErrorMessage = "UserId là bắt buộc")]
        public string UserId { get; set; } = null!;
    }
}