namespace AutoApiTester.Models.DTOs
{
    // simple key/value pair for headers/params
    public class KeyValuePairDto
    {
        public string? Key { get; set; }
        public string? Value { get; set; }
    }

    // request body DTO
    //public class RequestBodyDto
    //{
    //    public string? BodyType { get; set; }
    //    public string? Content { get; set; }
    //}

    // DTO returned to clients for a Request
    public class RequestResponseDto
    {
        public int Id { get; set; }
        public int CollectionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? AuthType { get; set; }
        public string? AuthValue { get; set; }
        public DateTime CreatedAt { get; set; }

        // ✅ dữ liệu con
        public List<KeyValuePairDto>? Headers { get; set; }
        public List<KeyValuePairDto>? QueryParams { get; set; }
        public List<RequestBodyDto>? Bodies { get; set; }

        // ✅ thêm trường test data
        public string? TestDataName { get; set; }
        public string? TestDataContent { get; set; }
    }

    // DTO used to create a request
    public class CreateRequestDto
    {
        public int CollectionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? AuthType { get; set; }
        public string? AuthValue { get; set; }

        public List<KeyValuePairDto>? Headers { get; set; }
        public List<KeyValuePairDto>? QueryParams { get; set; }
        // you can accept multiple bodies (e.g., different parts), but usually one. Keep as list for flexibility.
        public List<RequestBodyDto>? Bodies { get; set; }
    }

    // DTO used to update a request (same shape as create)
    public class UpdateRequestDto : CreateRequestDto
    {
    }

    // RequestDtDto used in exports (fix missing TestData property)
    public class RequestDtDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // nested lists
        public List<KeyValuePairDto>? Headers { get; set; }
        public List<KeyValuePairDto>? QueryParams { get; set; }
        public List<RequestBodyDto>? Bodies { get; set; }

        // Test data (file entries)
        public List<TestDataDto>? TestData { get; set; } = new();
    }

    // keep TestDataDto if you use it anywhere else
    public class TestDataDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
    public class UpdateTestDataRequestDto
    {
        public int RequestId { get; set; }
        public string NewTestDataContent { get; set; }
    }

}
