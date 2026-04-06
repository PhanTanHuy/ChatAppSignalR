using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppSignalR.DTOs
{
    public class RemoveMemberRequest
    {
        [FromRoute(Name = "userId")]
        [Required(ErrorMessage = "UserId là bắt buộc")]
        public string UserId { get; set; } = null!;
    }
}
