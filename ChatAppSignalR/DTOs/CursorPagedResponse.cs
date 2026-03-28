namespace ChatAppSignalR.DTOs
{
    public class CursorPagedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public DateTime? NextCursor { get; set; }
        public bool HasMore { get; set; }
        public int Limit { get; set; }
    }
}