namespace AutoApiTester.Models
{
    public class RequestHeaderEntity
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }

        // Navigation
        public RequestEntity Request { get; set; } = null!;
    }
}
