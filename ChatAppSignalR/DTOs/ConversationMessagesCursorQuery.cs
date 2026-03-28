using System.ComponentModel.DataAnnotations;

namespace ChatAppSignalR.DTOs
{
    public class ConversationMessagesCursorQuery
    {
        public DateTime? Cursor { get; set; }

        [Range(1, 100, ErrorMessage = "Limit phải từ 1 đến 100")]
        public int Limit { get; set; } = 30;
    }
}