using System.ComponentModel.DataAnnotations;

namespace ChatAppSignalR.DTOs
{
    public class SendMessageRequest : IValidatableObject
    {
        [Required(ErrorMessage = "ConversationId là bắt buộc")]
        public string ConversationId { get; set; } = null!;

        public string? Content { get; set; }

        public string? ImgUrl { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var hasContent = !string.IsNullOrWhiteSpace(Content);
            var hasImgUrl = !string.IsNullOrWhiteSpace(ImgUrl);

            if (!hasContent && !hasImgUrl)
            {
                yield return new ValidationResult("Phải có Content hoặc ImgUrl", new[] { nameof(Content), nameof(ImgUrl) });
            }
        }
    }
}