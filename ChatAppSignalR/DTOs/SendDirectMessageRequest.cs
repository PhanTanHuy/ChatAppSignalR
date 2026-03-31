using System.ComponentModel.DataAnnotations;

namespace ChatAppSignalR.DTOs
{
    public class SendDirectMessageRequest : IValidatableObject
    {
        [Required(ErrorMessage = "RecipientId là bắt buộc")]
        public string RecipientId { get; set; } = null!;

        public string? ConversationId { get; set; }

        public string? Content { get; set; }

        public string? ImgUrl { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var hasContent = !string.IsNullOrWhiteSpace(Content);
            var hasImgUrl = !string.IsNullOrWhiteSpace(ImgUrl);

            if (!hasContent && !hasImgUrl)
            {
                yield return new ValidationResult(
                    "Content và ImgUrl không được đồng thời rỗng",
                    new[] { nameof(Content), nameof(ImgUrl) }
                );
            }
        }
    }
}