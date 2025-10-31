namespace AutoApiTester.Models
{
    public class RequestBody
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string? BodyType { get; set; }  // none, raw, form-data, ...
        public string? Value { get; set; }
        public string? Type { get; set; }


        // Navigation
        public Request Request { get; set; } = null!;
    }
}
